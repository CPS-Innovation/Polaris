using Common.Services.CaseSearchService.Contracts;
using Common.Telemetry.Contracts;
using coordinator.Domain;
using coordinator.Functions.DurableEntity.Entity;
using coordinator.Functions.Orchestration.Functions.Case;
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
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
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
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ISearchIndexService searchIndexService,
        ITelemetryClient telemetryClient,
        IPolarisBlobStorageService blobStorageService
        )
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _searchIndexService = searchIndexService;
        _telemetryClient = telemetryClient;
        _blobStorageService = blobStorageService;
    }

    public async Task<string> FindCaseInstanceByDateAsync(DateTime createdTimeTo, Guid correlationId)
    {
        var caseId = string.Empty;
        var clearDownCandidates = new[] { OrchestrationRuntimeStatus.Completed, OrchestrationRuntimeStatus.Failed };

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

            var results = JsonConvert.DeserializeObject<List<DurableInstanceDto>>(jsonString);
            var targetInstance = results.FirstOrDefault(i => i.Name == nameof(RefreshCaseOrchestrator) && i.RuntimeStatus.IsClearDownCandidate(clearDownCandidates));

            if (targetInstance != null && !string.IsNullOrWhiteSpace(targetInstance.InstanceId))
                caseId = targetInstance.InstanceId;
        }
        catch (HttpRequestException exception)
        {
            if (exception.StatusCode == HttpStatusCode.NotFound)
                return string.Empty;

            throw;
        }
        return caseId;
    }

    public async Task<HttpResponseMessage> RefreshCaseAsync(IDurableOrchestrationClient orchestrationClient, Guid correlationId,
        string caseId, CaseOrchestrationPayload casePayload, HttpRequestMessage req)
    {
        var instanceId = RefreshCaseOrchestrator.GetKey(caseId);
        var existingInstance = await orchestrationClient.GetStatusAsync(instanceId);
        var isSingletonRefreshRunning = IsSingletonRefreshRunning(existingInstance);
        
        if (isSingletonRefreshRunning)
        {
            return new HttpResponseMessage(HttpStatusCode.Locked);
        }

        await orchestrationClient.StartNewAsync(nameof(RefreshCaseOrchestrator), instanceId, casePayload);

        return orchestrationClient.CreateCheckStatusResponse(req, instanceId);
    }

    public async Task<HttpResponseMessage> DeleteCaseAsync(IDurableOrchestrationClient orchestrationClient, Guid correlationId,
        int caseId, bool checkForBlobProtection)
    {
        var telemetryEvent = new DeletedCaseEvent(
                correlationId: correlationId,
                caseId: caseId,
                startTime: DateTime.UtcNow
        );
        var caseIdAsString = caseId.ToString();

        try
        {
            if (!_configuration.IsConfigSettingEnabled(FeatureFlags.DisableTextExtractorFeatureFlag))
            {
                await _searchIndexService.RemoveCaseIndexEntriesAsync(caseId, correlationId);
                telemetryEvent.RemovedCaseIndexTime = DateTime.UtcNow;

                await _searchIndexService.WaitForCaseEmptyResultsAsync(caseId, correlationId);
                telemetryEvent.IndexSettledTime = DateTime.UtcNow;
            }

            if (checkForBlobProtection)
            {
                if (!_configuration.IsSettingEnabled(ConfigKeys.CoordinatorKeys.SlidingClearDownProtectBlobs))
                {
                    await _blobStorageService.DeleteBlobsByCaseAsync(caseIdAsString, correlationId);
                }
            }
            else
            {
                await _blobStorageService.DeleteBlobsByCaseAsync(caseIdAsString, correlationId);
            }
            telemetryEvent.BlobsDeletedTime = DateTime.UtcNow;

            var terminateOrchestrationQueries = GetOrchestrationQueries(_terminateStatuses, caseIdAsString);
            var terminateOrchestrationInstanceIds = await TerminateOrchestrations(orchestrationClient, terminateOrchestrationQueries);
            telemetryEvent.TerminatedInstancesCount = terminateOrchestrationInstanceIds.Count;
            telemetryEvent.GotTerminateInstancesTime = DateTime.UtcNow;
            telemetryEvent.TerminatedInstancesTime = DateTime.UtcNow;

            // Purge Orchestrations and Durable Entities
            var purgeConditions = GetOrchestrationQueries(_purgeStatuses, caseIdAsString);
            purgeConditions.AddRange(GetDurableEntityQueries(_terminateStatuses, caseIdAsString));
            var success = await Purge(orchestrationClient, purgeConditions);
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

    private static async Task<List<string>> TerminateOrchestrations(IDurableOrchestrationClient client, List<OrchestrationStatusQueryCondition> terminateConditions)
    {
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

        return instanceIds;
    }

    private static async Task WaitForOrchestrationsToTerminateTask(IDurableOrchestrationClient client, IReadOnlyCollection<string> instanceIds)
    {
        if (instanceIds == null || !instanceIds.Any()) return;

        const int totalWaitTimeSeconds = 600;
        const int retryDelayMilliseconds = 1000;

        for (var i = 0; i < totalWaitTimeSeconds * 1000 / retryDelayMilliseconds; i++)
        {
            var statuses = await client.GetStatusAsync(instanceIds);

            if (statuses == null || statuses.All(status => status.RuntimeStatus == OrchestrationRuntimeStatus.Terminated)) return;

            await Task.Delay(retryDelayMilliseconds);
        }
    }

    private static async Task<bool> Purge(IDurableOrchestrationClient client, List<OrchestrationStatusQueryCondition> purgeConditions)
    {
        foreach (var purgeCondition in purgeConditions)
        {
            do
            {
                var statusQueryResult = await client.ListInstancesAsync(purgeCondition, CancellationToken.None);
                purgeCondition.ContinuationToken = statusQueryResult.ContinuationToken;

                var instancesToPurge = statusQueryResult.DurableOrchestrationState.Select(o => o.InstanceId).ToList();

                if (instancesToPurge.Any())
                    await client.PurgeInstanceHistoryAsync(instancesToPurge);
            } while (purgeCondition.ContinuationToken != null);

        }
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

        return conditions;
    }
}