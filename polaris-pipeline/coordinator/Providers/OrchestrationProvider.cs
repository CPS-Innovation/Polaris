using Common.Telemetry.Contracts;
using coordinator.Domain;
using coordinator.Functions.DurableEntity.Entity;
using coordinator.TelemetryEvents;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Common.Constants;
using Common.Services.BlobStorageService.Contracts;
using Microsoft.Extensions.Configuration;
using coordinator.Factories;
using System.Text.RegularExpressions;
using coordinator.Clients.Contracts;
using coordinator.Functions.Orchestration;

namespace coordinator.Providers;

public class OrchestrationProvider : IOrchestrationProvider
{
    private readonly IConfiguration _configuration;
    private readonly ITelemetryClient _telemetryClient;
    private readonly IPolarisBlobStorageService _blobStorageService;
    private readonly IQueryConditionFactory _queryConditionFactory;
    private readonly ITextExtractorClient _textExtractorClient;

    private static readonly OrchestrationRuntimeStatus[] _inProgressStatuses = {
        OrchestrationRuntimeStatus.Running,
        OrchestrationRuntimeStatus.Pending,
        OrchestrationRuntimeStatus.Suspended,
        OrchestrationRuntimeStatus.ContinuedAsNew
    };

    private static readonly OrchestrationRuntimeStatus[] _completedStatuses = {
        OrchestrationRuntimeStatus.Completed,
        OrchestrationRuntimeStatus.Canceled,
        OrchestrationRuntimeStatus.Failed,
        OrchestrationRuntimeStatus.Terminated
    };

    private static readonly OrchestrationRuntimeStatus[] _entityStatuses = {
        // entities are eternally running orchestrations
        OrchestrationRuntimeStatus.Running,
    };

    public OrchestrationProvider(
            IConfiguration configuration,
            ITelemetryClient telemetryClient,
            IPolarisBlobStorageService blobStorageService,
            IQueryConditionFactory queryConditionFactory,
            ITextExtractorClient textExtractorClient
    )
    {
        _configuration = configuration;
        _telemetryClient = telemetryClient;
        _blobStorageService = blobStorageService;
        _queryConditionFactory = queryConditionFactory;
        _textExtractorClient = textExtractorClient;
    }

    public async Task<List<int>> FindCaseInstancesByDateAsync(IDurableOrchestrationClient orchestrationClient, DateTime createdTimeTo, int batchSize)
    {
        var instanceIds = await GetInstanceIdsAsync(orchestrationClient,
            _queryConditionFactory.Create(createdTimeTo, batchSize),
            shouldFollowContinuation: false
        );

        static int getCaseIdFromInstanceId(string instanceId) => int.Parse(
            Regex.Match(instanceId, @"\d+", RegexOptions.None, TimeSpan.FromSeconds(1))
            .Value
        );

        return instanceIds
            .Select(getCaseIdFromInstanceId)
            .ToList();
    }

    public async Task<HttpResponseMessage> RefreshCaseAsync(IDurableOrchestrationClient client, Guid correlationId,
        string caseId, CaseOrchestrationPayload casePayload, HttpRequestMessage req)
    {
        var instanceId = RefreshCaseOrchestrator.GetKey(caseId);
        var existingInstance = await client.GetStatusAsync(instanceId);

        if (existingInstance != null && _inProgressStatuses.Contains(existingInstance.RuntimeStatus))
        {
            return new HttpResponseMessage(HttpStatusCode.Locked);
        }

        await client.StartNewAsync(nameof(RefreshCaseOrchestrator), instanceId, casePayload);
        return client.CreateCheckStatusResponse(req, instanceId);
    }

    public async Task DeleteCaseAsync(IDurableOrchestrationClient client,
                                      Guid correlationId,
                                      string caseUrn,
                                      int caseId,
                                      bool checkForBlobProtection = true,
                                      bool waitForIndexToSettle = true)
    {
        var telemetryEvent = new DeletedCaseEvent(
            correlationId: correlationId,
            caseId: caseId,
            startTime: DateTime.UtcNow
        );

        try
        {
            var deleteResult = await _textExtractorClient.RemoveCaseIndexesAsync(caseUrn, caseId, correlationId);
            telemetryEvent.RemovedCaseIndexTime = DateTime.UtcNow;
            telemetryEvent.AttemptedRemovedDocumentCount = deleteResult.DocumentCount;
            telemetryEvent.SuccessfulRemovedDocumentCount = deleteResult.SuccessCount;
            telemetryEvent.FailedRemovedDocumentCount = deleteResult.FailureCount;

            if (waitForIndexToSettle)
            {
                var waitResult = await _textExtractorClient.WaitForCaseEmptyResultsAsync(caseUrn, caseId, correlationId);
                telemetryEvent.DidIndexSettle = waitResult.IsSuccess;
                telemetryEvent.WaitRecordCounts = waitResult.RecordCounts;
                telemetryEvent.IndexSettledTime = DateTime.UtcNow;
            }
            telemetryEvent.DidWaitForIndexToSettle = waitForIndexToSettle;

            var shouldClearBlobs = !checkForBlobProtection
                || !_configuration.IsSettingEnabled(ConfigKeys.CoordinatorKeys.SlidingClearDownProtectBlobs);
            if (shouldClearBlobs)
            {
                await _blobStorageService.DeleteBlobsByCaseAsync(caseId.ToString());
                telemetryEvent.BlobsDeletedTime = DateTime.UtcNow;
            }
            telemetryEvent.DidClearBlobs = shouldClearBlobs;

            var terminateInstanceIds = await GetInstanceIdsAsync(client,
                _queryConditionFactory.Create(_inProgressStatuses, RefreshCaseOrchestrator.GetKey(caseId.ToString()))
             );
            telemetryEvent.TerminatedInstancesCount = terminateInstanceIds.Count;
            telemetryEvent.GotTerminateInstancesTime = DateTime.UtcNow;

            await Task.WhenAll(
                terminateInstanceIds.Select(instanceId => client.TerminateAsync(instanceId, "Forcibly terminated DELETE"))
            );
            telemetryEvent.TerminatedInstancesTime = DateTime.UtcNow;

            var didComplete = await WaitForOrchestrationsToCompleteAsync(client, terminateInstanceIds);
            telemetryEvent.DidOrchestrationsTerminate = didComplete;
            telemetryEvent.TerminatedInstancesSettledTime = DateTime.UtcNow;

            var orchestratorPurgeInstanceIds = await GetInstanceIdsAsync(client,
                 _queryConditionFactory.Create(_completedStatuses, RefreshCaseOrchestrator.GetKey(caseId.ToString()))
            );
            var entityPurgeInstanceIds = await GetInstanceIdsAsync(client,
                 _queryConditionFactory.Create(_entityStatuses, CaseDurableEntity.GetInstanceId(caseId.ToString()))
            );
            telemetryEvent.GotPurgeInstancesTime = DateTime.UtcNow;
            var instancesToPurge = Enumerable.Concat(orchestratorPurgeInstanceIds, entityPurgeInstanceIds);
            telemetryEvent.PurgeInstancesCount = instancesToPurge.Count();

            var result = await client.PurgeInstanceHistoryAsync(instancesToPurge);
            telemetryEvent.PurgedInstancesCount = result.InstancesDeleted;

            telemetryEvent.EndTime = DateTime.UtcNow;
            _telemetryClient.TrackEvent(telemetryEvent);
        }
        catch (Exception)
        {
            _telemetryClient.TrackEventFailure(telemetryEvent);
            throw;
        }
    }

    private static async Task<List<string>> GetInstanceIdsAsync(IDurableOrchestrationClient client, OrchestrationStatusQueryCondition condition, bool shouldFollowContinuation = true)
    {
        var instanceIds = new List<string>();
        do
        {
            var statusQueryResult = await client.ListInstancesAsync(condition, CancellationToken.None);
            condition.ContinuationToken = statusQueryResult.ContinuationToken;

            instanceIds.AddRange(
                statusQueryResult.DurableOrchestrationState
                .Select(o => o.InstanceId)
            );
        }
        while (shouldFollowContinuation && condition.ContinuationToken != null);

        return instanceIds;
    }

    private static async Task<bool> WaitForOrchestrationsToCompleteAsync(IDurableOrchestrationClient client, IReadOnlyCollection<string> instanceIds)
    {
        int remainingRetryAttempts = 10;
        const int retryDelayMilliseconds = 1000;
        do
        {
            var statuses = await client.GetStatusAsync(instanceIds);
            if (statuses.All(status => status.RuntimeStatus == OrchestrationRuntimeStatus.Terminated))
            {
                return true;
            };

            await Task.Delay(retryDelayMilliseconds);
            remainingRetryAttempts--;

        } while (remainingRetryAttempts >= 0);

        return false;
    }
}