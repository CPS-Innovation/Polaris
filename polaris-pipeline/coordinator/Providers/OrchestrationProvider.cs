using Common.Services.CaseSearchService.Contracts;
using Common.Telemetry.Contracts;
using coordinator.Domain;
using coordinator.Functions.DurableEntity.Entity;
using coordinator.Functions.Orchestration.Functions.Case;
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

namespace coordinator.Providers;

public class OrchestrationProvider : IOrchestrationProvider
{
    private readonly IConfiguration _configuration;
    private readonly ISearchIndexService _searchIndexService;
    private readonly ITelemetryClient _telemetryClient;
    private readonly IPolarisBlobStorageService _blobStorageService;
    private readonly IQueryConditionFactory _queryConditionFactory;

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

    public OrchestrationProvider(
            IConfiguration configuration,
            ISearchIndexService searchIndexService,
            ITelemetryClient telemetryClient,
            IPolarisBlobStorageService blobStorageService,
            IQueryConditionFactory queryConditionFactory
    )
    {
        _configuration = configuration;
        _searchIndexService = searchIndexService;
        _telemetryClient = telemetryClient;
        _blobStorageService = blobStorageService;
        _queryConditionFactory = queryConditionFactory;
    }

    public async Task<List<int>> FindCaseInstancesByDateAsync(IDurableOrchestrationClient orchestrationClient, DateTime createdTimeTo, int batchSize)
    {
        var instanceIds = await GetInstanceIds(orchestrationClient,
            _queryConditionFactory.Create(createdTimeTo, batchSize)
        );

        return instanceIds.Select(i => int.Parse(i)).ToList();
    }

    public async Task<HttpResponseMessage> RefreshCaseAsync(IDurableOrchestrationClient client, Guid correlationId,
        string caseId, CaseOrchestrationPayload casePayload, HttpRequestMessage req)
    {
        var instanceId = RefreshCaseOrchestrator.GetKey(caseId);
        var existingInstance = await client.GetStatusAsync(instanceId);

        if (_inProgressStatuses.Contains(existingInstance.RuntimeStatus))
        {
            return new HttpResponseMessage(HttpStatusCode.Locked);
        }

        await client.StartNewAsync(nameof(RefreshCaseOrchestrator), instanceId, casePayload);
        return client.CreateCheckStatusResponse(req, instanceId);
    }

    public async Task DeleteCaseAsync(IDurableOrchestrationClient client,
                                      Guid correlationId,
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
            var deleteResult = await _searchIndexService.RemoveCaseIndexEntriesAsync(caseId);
            telemetryEvent.RemovedCaseIndexTime = DateTime.UtcNow;
            telemetryEvent.AttemptedRemovedDocumentCount = deleteResult.DocumentCount;
            telemetryEvent.SuccessfulRemovedDocumentCount = deleteResult.SuccessCount;
            telemetryEvent.FailedRemovedDocumentCount = deleteResult.FailureCount;

            if (waitForIndexToSettle)
            {
                var waitResult = await _searchIndexService.WaitForCaseEmptyResultsAsync(caseId);
                telemetryEvent.DidIndexSettle = waitResult.IsSuccess;
                telemetryEvent.WaitRecordCounts = waitResult.RecordCounts;
                telemetryEvent.IndexSettledTime = DateTime.UtcNow;
            }

            var shouldClearBlobs = !checkForBlobProtection
                || !_configuration.IsSettingEnabled(ConfigKeys.CoordinatorKeys.SlidingClearDownProtectBlobs);
            if (shouldClearBlobs)
            {
                await _blobStorageService.DeleteBlobsByCaseAsync(caseId.ToString());
                telemetryEvent.BlobsDeletedTime = DateTime.UtcNow;
            }

            var terminateInstanceIds = await GetInstanceIds(client,
                _queryConditionFactory.Create(_inProgressStatuses, RefreshCaseOrchestrator.GetKey(caseId.ToString()))
             );
            telemetryEvent.GotTerminateInstancesTime = DateTime.UtcNow;

            await Task.WhenAll(
                terminateInstanceIds.Select(instanceId => client.TerminateAsync(instanceId, "Forcibly terminated DELETE"))
            );
            telemetryEvent.TerminatedInstancesCount = terminateInstanceIds.Count;
            telemetryEvent.TerminatedInstancesTime = DateTime.UtcNow;

            var didTerminate = await WaitForOrchestrationsToTerminateTask(client, terminateInstanceIds);
            // todo: terminated settled time
            // did settle

            var orchestratorPurgeInstanceIds = await GetInstanceIds(client,
                 _queryConditionFactory.Create(_completedStatuses, RefreshCaseOrchestrator.GetKey(caseId.ToString()))
            );
            var entityPurgeInstanceIds = await GetInstanceIds(client,
                 _queryConditionFactory.Create(_completedStatuses, CaseDurableEntity.GetInstanceId(caseId.ToString()))
            );
            // todo: got purge time and counts
            var result = await client.PurgeInstanceHistoryAsync(Enumerable.Concat(orchestratorPurgeInstanceIds, entityPurgeInstanceIds));
            // todo: purged time

            telemetryEvent.EndTime = DateTime.UtcNow;
            _telemetryClient.TrackEvent(telemetryEvent);
        }
        catch (Exception)
        {
            _telemetryClient.TrackEventFailure(telemetryEvent);
            throw;
        }
    }

    private static async Task<List<string>> GetInstanceIds(IDurableOrchestrationClient client, OrchestrationStatusQueryCondition condition)
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
        while (condition.ContinuationToken != null);

        return instanceIds;
    }

    private static async Task<bool> WaitForOrchestrationsToTerminateTask(IDurableOrchestrationClient client, IReadOnlyCollection<string> instanceIds)
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