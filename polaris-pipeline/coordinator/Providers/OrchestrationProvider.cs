using Common.Domain.Exceptions;
using Common.Logging;
using Common.Services.CaseSearchService.Contracts;
using Common.Telemetry.Contracts;
using Common.Wrappers.Contracts;
using coordinator.Domain;
using coordinator.Functions.DurableEntity.Entity;
using coordinator.Functions.Orchestration.Functions.Case;
using coordinator.Functions.Orchestration.Functions.Tracker;
using coordinator.TelemetryEvents;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using Common.Services.BlobStorageService.Contracts;
using coordinator.Domain.Dto;
using coordinator.Domain.Extensions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace coordinator.Providers;

public class OrchestrationProvider : IOrchestrationProvider
{
    private readonly ILogger<OrchestrationProvider> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IJsonConvertWrapper _jsonConvertWrapper;
    private readonly ISearchIndexService _searchIndexService;
    private readonly ITelemetryClient _telemetryClient;
    private readonly IPolarisBlobStorageService _blobStorageService;
    
    private const int DefaultPageSize = 100;

    private readonly OrchestrationRuntimeStatus[] _terminateStatuses = {
        OrchestrationRuntimeStatus.Running,
        OrchestrationRuntimeStatus.Pending,
        OrchestrationRuntimeStatus.Suspended,
        OrchestrationRuntimeStatus.ContinuedAsNew
    };

    private readonly OrchestrationRuntimeStatus[] _purgeStatuses = {
        OrchestrationRuntimeStatus.Completed,
        OrchestrationRuntimeStatus.Canceled,
        OrchestrationRuntimeStatus.Failed,
        OrchestrationRuntimeStatus.Terminated
    };
    
    public OrchestrationProvider(
        ILogger<OrchestrationProvider> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IJsonConvertWrapper jsonConvertWrapper,
        ISearchIndexService searchIndexService,
        ITelemetryClient telemetryClient,
        IPolarisBlobStorageService blobStorageService
        )
    {
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _jsonConvertWrapper = jsonConvertWrapper;
        _searchIndexService = searchIndexService;
        _telemetryClient = telemetryClient;
        _blobStorageService = blobStorageService;
    }
    
    public async Task<string> FindCaseInstanceByDateAsync(DateTime createdTimeTo, Guid correlationId)
    {
        _logger.LogMethodEntry(correlationId, nameof(FindCaseInstanceByDateAsync), $"Searching durable instance records for target case to clear-down, up to '{createdTimeTo:dd-MM-yyyy}'");

        var caseId = string.Empty;
        var clearDownCandidates = new[] {OrchestrationRuntimeStatus.Completed, OrchestrationRuntimeStatus.Failed};
        
        try
        {
            var requestUri = $"{RestApi.GetInstancesPath()}?code={_configuration[PipelineSettings.PipelineCoordinatorDurableExtensionCode]}&createdTimeTo={createdTimeTo:yyyy-MM-ddThh:mm:ss.fffZ}&showInput=false&showHistoryOutput=false&instanceIdPrefix=[";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());
            request.Content = null;
            const string httpClientName = $"Low-level{nameof(OrchestrationProvider)}";
            var httpClient = _httpClientFactory.CreateClient(httpClientName);

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            var jsonString = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(jsonString))
                return caseId;
            
            var results =  JsonConvert.DeserializeObject<List<DurableInstanceDto>>(jsonString);
            var targetInstance = results.FirstOrDefault(i => i.Name == Orchestrators.RefreshCaseOrchestrator && i.RuntimeStatus.IsClearDownCandidate(clearDownCandidates));
            
            if (targetInstance != null && !string.IsNullOrWhiteSpace(targetInstance.InstanceId))
                caseId = targetInstance.InstanceId;
        }
        catch (HttpRequestException exception)
        {
            if (exception.StatusCode == HttpStatusCode.NotFound)
                return string.Empty;
                
            throw;
        }

        _logger.LogMethodExit(correlationId, nameof(FindCaseInstanceByDateAsync), string.Empty);

        return caseId;
    }
    
    public async Task<HttpResponseMessage> RefreshCaseAsync(IDurableOrchestrationClient orchestrationClient, Guid correlationId, 
        string caseId, CaseOrchestrationPayload casePayload, HttpRequestMessage req)
    {
        var instanceId = RefreshCaseOrchestrator.GetKey(caseId);
        var existingInstance = await orchestrationClient.GetStatusAsync(instanceId);
        var isSingletonRefreshRunning = IsSingletonRefreshRunning(existingInstance);
        const string loggingName = $"{nameof(OrchestrationProvider)} - {nameof(RefreshCaseAsync)}";

        if (isSingletonRefreshRunning)
        {
            _logger.LogMethodFlow(correlationId, loggingName, $"{nameof(OrchestrationProvider)} Locked as already running - {nameof(RefreshCaseOrchestrator)} with instance id '{caseId}'");
            return new HttpResponseMessage(HttpStatusCode.Locked);
        }

        await orchestrationClient.StartNewAsync(nameof(RefreshCaseOrchestrator), instanceId, casePayload);

        _logger.LogMethodFlow(correlationId, loggingName, $"{nameof(OrchestrationProvider)} Succeeded - Started {nameof(RefreshCaseOrchestrator)} with instance id '{instanceId}'");
        return orchestrationClient.CreateCheckStatusResponse(req, instanceId);
    }

    public async Task<HttpResponseMessage> DeleteCaseAsync(IDurableOrchestrationClient orchestrationClient, Guid correlationId, 
        int caseId)
    {
        var telemetryEvent = new DeletedCaseEvent(
                correlationId: correlationId,
                caseId: caseId,
                startTime: DateTime.UtcNow
        );
        var caseIdAsString = caseId.ToString();
        
        try
        {
            await _searchIndexService.RemoveCaseIndexEntriesAsync(caseId, correlationId);
            telemetryEvent.RemovedCaseIndexTime = DateTime.UtcNow;

            await _searchIndexService.WaitForCaseEmptyResultsAsync(caseId, correlationId);
            telemetryEvent.IndexSettledTime = DateTime.UtcNow;
            
            await _blobStorageService.DeleteBlobsByCaseAsync(caseIdAsString, correlationId);
            telemetryEvent.BlobsDeletedTime = DateTime.UtcNow;

            // Terminate Orchestrations (can't terminate Durable Entities with Netherite backend, but can Purge - see below)
            var terminateOrchestrationQueries = GetOrchestrationQueries(_terminateStatuses, caseIdAsString);
            var terminateOrchestrationInstanceIds = await TerminateOrchestrations(orchestrationClient, terminateOrchestrationQueries, correlationId);
            telemetryEvent.TerminatedInstancesCount = terminateOrchestrationInstanceIds.Count;
            telemetryEvent.GotTerminateInstancesTime = DateTime.UtcNow;
            telemetryEvent.TerminatedInstancesTime = DateTime.UtcNow;

            // Purge Orchestrations and Durable Entities
            var purgeConditions = GetOrchestrationQueries(_purgeStatuses, caseIdAsString);
            purgeConditions.AddRange(GetDurableEntityQueries(_terminateStatuses, caseIdAsString));
            var success = await Purge(orchestrationClient, purgeConditions, correlationId);
            telemetryEvent.EndTime = DateTime.UtcNow;

            _telemetryClient.TrackEvent(telemetryEvent);

            return new HttpResponseMessage(success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);
        }
        catch (Exception)
        {
            _telemetryClient.TrackEventFailure(telemetryEvent);
            throw;
        }
    }
    
    private static bool IsSingletonRefreshRunning(DurableOrchestrationStatus existingInstance)
    {
        // https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-singletons?tabs=csharp
        var notRunning = existingInstance == null ||
                         existingInstance.RuntimeStatus
                             is OrchestrationRuntimeStatus.Completed
                             or OrchestrationRuntimeStatus.Failed
                             or OrchestrationRuntimeStatus.Terminated
                             // Is this correct? unit tests assert Canceled state, but MS docs don't include this state
                             or OrchestrationRuntimeStatus.Canceled;

        return !notRunning;
    }
    
    private async Task<List<string>> TerminateOrchestrations(IDurableOrchestrationClient client, List<OrchestrationStatusQueryCondition> terminateConditions, Guid correlationId)
    {
        const string loggingName = $"{nameof(OrchestrationProvider)} - {nameof(TerminateOrchestrations)}";
        _logger.LogMethodFlow(correlationId, loggingName, "Terminating Case Orchestrations and Document Sub Orchestrations");

        var instanceIds = new List<string>();
        foreach (var terminateCondition in terminateConditions)
        {
            do
            {
                var statusQueryResult = await client.ListInstancesAsync(terminateCondition, CancellationToken.None);
                terminateCondition.ContinuationToken = statusQueryResult.ContinuationToken;

                var instancesToTerminate = statusQueryResult.DurableOrchestrationState.Select(o => o.InstanceId).ToList();

                if (!instancesToTerminate.Any()) continue;
                instanceIds.AddRange(instancesToTerminate);
                await Task.WhenAll(instancesToTerminate.Select(async instanceId => await client.TerminateAsync(instanceId, "Forcibly terminated DELETE")));
            }
            while (terminateCondition.ContinuationToken != null);
        }

        await WaitForOrchestrationsToTerminateTask(client, instanceIds);

        _logger.LogMethodFlow(correlationId, loggingName, $"Terminating {instanceIds.Count} Case Orchestrations and Document Sub Orchestrations completed");

        return instanceIds;
    }
    
    private static async Task WaitForOrchestrationsToTerminateTask(IDurableOrchestrationClient client, IReadOnlyCollection<string> instanceIds)
    {
        if(instanceIds == null || !instanceIds.Any()) return;

        const int totalWaitTimeSeconds = 600;   
        const int retryDelayMilliseconds = 1000;

        for (var i = 0; i < (totalWaitTimeSeconds * 1000) / retryDelayMilliseconds; i++)
        {
            var statuses = await client.GetStatusAsync(instanceIds);

            if (statuses == null || statuses.All(status => status.RuntimeStatus == OrchestrationRuntimeStatus.Terminated)) return;

            await Task.Delay(retryDelayMilliseconds);
        }
    }
    
    private async Task<bool> Purge(IDurableOrchestrationClient client, List<OrchestrationStatusQueryCondition> purgeConditions, Guid correlationId)
    {
        const string loggingName = $"{nameof(OrchestrationProvider)} - {nameof(Purge)}";
        _logger.LogMethodFlow(correlationId, loggingName, "Purging Case Orchestrations, Sub Orchestrations and Durable Entities");

        foreach (var purgeCondition in purgeConditions)
        {
            do
            {
                var statusQueryResult = await client.ListInstancesAsync(purgeCondition, CancellationToken.None);
                purgeCondition.ContinuationToken = statusQueryResult.ContinuationToken;

                var instancesToPurge = statusQueryResult.DurableOrchestrationState.Select(o => o.InstanceId).ToList();

                if(instancesToPurge.Any())
                    await client.PurgeInstanceHistoryAsync(instancesToPurge);
            } while (purgeCondition.ContinuationToken != null);

        }
        _logger.LogMethodFlow(correlationId, loggingName, $"Purging Case Orchestrations, Sub Orchestrations and Durable Entities completed");

        return true;
    }

    private static List<OrchestrationStatusQueryCondition> GetOrchestrationQueries(IEnumerable<OrchestrationRuntimeStatus> runtimeStatuses, string caseId)
    {
        var conditions = new List<OrchestrationStatusQueryCondition>();

        // RefreshCaseOrchestrator, instanceId = e.g. [2149310]
        // RefreshDocumentOrchestrator, instanceId = e.g. [2149310]-CMS4284166
        // Common root = e.g. [2149310]
        var refreshCaseOrDocumentOrchestratorCondition = new OrchestrationStatusQueryCondition
        {
            InstanceIdPrefix = RefreshCaseOrchestrator.GetKey(caseId),
            CreatedTimeFrom = DateTime.MinValue,
            CreatedTimeTo = DateTime.UtcNow,
            RuntimeStatus = runtimeStatuses,
            PageSize = DefaultPageSize,
            ContinuationToken = null
        };
        conditions.Add(refreshCaseOrDocumentOrchestratorCondition);

        return conditions;
    }
    
    private static IEnumerable<OrchestrationStatusQueryCondition> GetDurableEntityQueries(IEnumerable<OrchestrationRuntimeStatus> runtimeStatuses, string caseId)
    {
        var conditions = new List<OrchestrationStatusQueryCondition>();
        var statusesList = runtimeStatuses.ToList();

        // e.g. @casedurableentity@[2149310]
        var caseDurableEntityCondition = new OrchestrationStatusQueryCondition
        {
            InstanceIdPrefix = CaseDurableEntity.GetInstanceId(caseId),
            CreatedTimeFrom = DateTime.MinValue,
            CreatedTimeTo = DateTime.UtcNow,
            RuntimeStatus = statusesList,
            PageSize = DefaultPageSize,
            ContinuationToken = null
        };
        conditions.Add(caseDurableEntityCondition);

        // e.g @caserefreshlogsdurableentity@[2149310]-1
        var caseRefreshLogsDurableEntitiesCondition = new OrchestrationStatusQueryCondition
        {
            InstanceIdPrefix = CaseRefreshLogsDurableEntity.GetInstanceId(caseId, 0).Replace("-0", string.Empty),
            CreatedTimeFrom = DateTime.MinValue,
            CreatedTimeTo = DateTime.UtcNow,
            RuntimeStatus = statusesList,
            PageSize = DefaultPageSize,
            ContinuationToken = null
        };
        conditions.Add(caseRefreshLogsDurableEntitiesCondition); 

        return conditions;
    }
}