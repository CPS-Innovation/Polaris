using coordinator.Durable.Entity;
using coordinator.Durable.Orchestration;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using Common.Dto.Response;
using coordinator.Durable.Payloads;
using Microsoft.AspNetCore.Http;

namespace coordinator.Durable.Providers;

public class OrchestrationProvider : IOrchestrationProvider
{
    private readonly IConfiguration _configuration;
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

    private static readonly OrchestrationRuntimeStatus[] _entityStatuses = {
        // entities are eternally running orchestrations
        OrchestrationRuntimeStatus.Running,
    };

    public OrchestrationProvider(
            IConfiguration configuration,
            IQueryConditionFactory queryConditionFactory
    )
    {
        _configuration = configuration;
        _queryConditionFactory = queryConditionFactory;
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

    public async Task<bool> RefreshCaseAsync(IDurableOrchestrationClient client, Guid correlationId,
        string caseId, CaseOrchestrationPayload casePayload, HttpRequest req)
    {
        var instanceId = RefreshCaseOrchestrator.GetKey(caseId);
        var existingInstance = await client.GetStatusAsync(instanceId);

        if (existingInstance != null && _inProgressStatuses.Contains(existingInstance.RuntimeStatus))
        {
            return false;
        }

        await client.StartNewAsync(nameof(RefreshCaseOrchestrator), instanceId, casePayload);
        return true;
    }

    public async Task<DeleteCaseOrchestrationResult> DeleteCaseOrchestrationAsync(IDurableOrchestrationClient client, int caseId)
    {
        var result = new DeleteCaseOrchestrationResult();
        try
        {
            var terminateInstanceIds = await GetInstanceIdsAsync(client,
                _queryConditionFactory.Create(_inProgressStatuses, RefreshCaseOrchestrator.GetKey(caseId.ToString()))
             );
            result.TerminatedInstancesCount = terminateInstanceIds.Count;
            result.GotTerminateInstancesDateTime = DateTime.UtcNow;

            await Task.WhenAll(
                terminateInstanceIds.Select(instanceId => client.TerminateAsync(instanceId, "Forcibly terminated DELETE"))
            );
            result.TerminatedInstancesTime = DateTime.UtcNow;

            var didComplete = await WaitForOrchestrationsToCompleteAsync(client, terminateInstanceIds);
            result.DidOrchestrationsTerminate = didComplete;
            result.TerminatedInstancesSettledDateTime = DateTime.UtcNow;

            var orchestratorPurgeInstanceIds = await GetInstanceIdsAsync(client,
                 _queryConditionFactory.Create(_completedStatuses, RefreshCaseOrchestrator.GetKey(caseId.ToString()))
            );
            var entityPurgeInstanceIds = await GetInstanceIdsAsync(client,
                 _queryConditionFactory.Create(_entityStatuses, CaseDurableEntity.GetInstanceId(caseId.ToString()))
            );
            result.GotPurgeInstancesDateTime = DateTime.UtcNow;
            var instancesToPurge = Enumerable.Concat(orchestratorPurgeInstanceIds, entityPurgeInstanceIds);
            result.PurgeInstancesCount = instancesToPurge.Count();

            var purgeResult = await client.PurgeInstanceHistoryAsync(instancesToPurge);
            result.PurgedInstancesCount = purgeResult.InstancesDeleted;

            result.OrchestrationEndDateTime = DateTime.UtcNow;
            result.IsSuccess = true;
            return result;
        }
        catch (Exception)
        {
            return result;
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