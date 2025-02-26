using coordinator.Durable.Orchestration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using Common.Dto.Response;
using coordinator.Durable.Payloads;
using Microsoft.AspNetCore.Http;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Durable.Providers;

public class OrchestrationProvider : IOrchestrationProvider
{
    private readonly IConfiguration _configuration;
    private readonly IQueryConditionFactory _queryConditionFactory;
    private readonly ILogger<OrchestrationProvider> _logger;
    private static readonly OrchestrationRuntimeStatus[] _inProgressStatuses =
    [
        OrchestrationRuntimeStatus.Running,
        OrchestrationRuntimeStatus.Pending,
        OrchestrationRuntimeStatus.Suspended,
    ];

    private static readonly OrchestrationRuntimeStatus[] _completedStatuses =
    [
        OrchestrationRuntimeStatus.Completed,
        OrchestrationRuntimeStatus.Failed,
        OrchestrationRuntimeStatus.Terminated
    ];

    private static readonly OrchestrationRuntimeStatus[] _entityStatuses =
    [
        // entities are eternally running orchestrations
        OrchestrationRuntimeStatus.Running,
    ];

    public OrchestrationProvider(
            IConfiguration configuration,
            IQueryConditionFactory queryConditionFactory,
            ILogger<OrchestrationProvider> logger)
    {
        _configuration = configuration;
        _queryConditionFactory = queryConditionFactory;
        _logger = logger;
    }

    public static string GetKey(int caseId) => $"[{caseId}]";

    public async Task<List<int>> FindCaseInstancesByDateAsync(DurableTaskClient orchestrationClient, DateTime createdTimeTo, int batchSize)
    {
        var instanceIds = await GetInstanceIdsAsync(orchestrationClient, _queryConditionFactory.Create(createdTimeTo, batchSize));

        return instanceIds
            .Select(GetCaseIdFromInstanceId)
            .ToList();
    }

    public async Task<bool> RefreshCaseAsync(DurableTaskClient client, Guid correlationId,
        int caseId, CasePayload casePayload, HttpRequest req)
    {
        var instanceId = GetKey(caseId);
        var existingInstance = await client.GetInstanceAsync(instanceId);

        if (existingInstance != null && _inProgressStatuses.Contains(existingInstance.RuntimeStatus))
        {
            return false;
        }

        await client.ScheduleNewOrchestrationInstanceAsync(nameof(RefreshCaseOrchestrator), casePayload, new StartOrchestrationOptions { InstanceId = instanceId });
        return true;
    }

    public async Task<DeleteCaseOrchestrationResult> DeleteCaseOrchestrationAsync(DurableTaskClient client, int caseId)
    {
        var result = new DeleteCaseOrchestrationResult();
        try
        {
            var terminateInstanceIds = await GetInstanceIdsAsync(client, _queryConditionFactory.Create(_inProgressStatuses, GetKey(caseId)));
            result.TerminatedInstancesCount = terminateInstanceIds.Count;
            result.GotTerminateInstancesDateTime = DateTime.UtcNow;

            await Task.WhenAll(terminateInstanceIds.Select(instanceId => client.TerminateInstanceAsync(instanceId, "Forcibly terminated DELETE")));
            result.TerminatedInstancesTime = DateTime.UtcNow;

            var didComplete = await WaitForOrchestrationsToCompleteAsync(client);
            result.DidOrchestrationsTerminate = didComplete;
            result.TerminatedInstancesSettledDateTime = DateTime.UtcNow;

            var orchestratorPurgeInstanceIds = await GetInstanceIdsAsync(client, _queryConditionFactory.Create(_completedStatuses, GetKey(caseId)));

            result.GotPurgeInstancesDateTime = DateTime.UtcNow;
            result.PurgeInstancesCount = orchestratorPurgeInstanceIds.Count;

            foreach (var instance in orchestratorPurgeInstanceIds)
            {
                var purgeResult = await client.PurgeInstanceAsync(instance);
                result.PurgedInstancesCount += purgeResult.PurgedInstanceCount;
            }

            result.OrchestrationEndDateTime = DateTime.UtcNow;
            result.IsSuccess = true;
            return result;
        }
        catch (Exception)
        {
            return result;
        }
    }

    private static async Task<List<string>> GetInstanceIdsAsync(DurableTaskClient client, OrchestrationQuery condition)
    {
        var instanceIds = new List<string>();

        await foreach (var page in client.GetAllInstancesAsync(condition).AsPages())
        {
            instanceIds.AddRange(page.Values.Select(o => o.InstanceId));
        }

        return instanceIds;
    }

    private static async Task<bool> WaitForOrchestrationsToCompleteAsync(DurableTaskClient client)
    {
        int remainingRetryAttempts = 10;
        const int retryDelayMilliseconds = 1000;
        do
        {
            var allInstancesAreTerminated = true;
            await foreach (var page in client.GetAllInstancesAsync(new OrchestrationQuery(Statuses: _inProgressStatuses)).AsPages())
            {
                allInstancesAreTerminated &= page.Values.All(i => i.RuntimeStatus == OrchestrationRuntimeStatus.Terminated);
            }

            if (allInstancesAreTerminated)
            {
                return true;
            }

            await Task.Delay(retryDelayMilliseconds);
            remainingRetryAttempts--;

        } while (remainingRetryAttempts >= 0);

        return false;
    }

    static int GetCaseIdFromInstanceId(string instanceId) => int.Parse(
        Regex.Match(instanceId, @"\d+", RegexOptions.None, TimeSpan.FromSeconds(1))
        .Value
    );
}