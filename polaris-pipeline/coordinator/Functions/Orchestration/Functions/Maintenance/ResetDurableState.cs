using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.Orchestration.Functions.Maintenance;

public class ResetDurableState
{
    private readonly ILogger<ResetDurableState> _logger;
    private const string LoggingName = $"{nameof(ResetDurableState)} - {nameof(RunAsync)}";
    private const int DefaultPageSize = 100;
    private const int MaxAzureFunctionRunTimeMinutes = 10;
    
    public ResetDurableState(ILogger<ResetDurableState> logger)
    {
        _logger = logger;
    }

    [FunctionName(nameof(ResetDurableState))]
    public async Task RunAsync(
        [TimerTrigger("%OvernightClearDownSchedule%", RunOnStartup = true)] TimerInfo myTimer,
        [DurableClient] IDurableOrchestrationClient client)
    {
        try
        {
            await TerminateOrchestrationsAndDurableEntities(client);
            await WaitForTerminationsToComplete();
            await PurgeOrchestrationsAndDurableEntitiesHistory(client);
        }
        catch (Exception ex)
        {
            _logger.LogMethodError(Guid.NewGuid(), LoggingName, ex.Message, ex);
        }
    }

    private static async Task WaitForTerminationsToComplete()
    {
        await Task.Delay(TimeSpan.FromMinutes(MaxAzureFunctionRunTimeMinutes + 1));
    }

    private static async Task TerminateOrchestrationsAndDurableEntities(IDurableOrchestrationClient client)
    {
        var terminateCondition = CreateOrchestrationQuery(new[]
        {
            OrchestrationRuntimeStatus.Running,
            OrchestrationRuntimeStatus.Pending,
            OrchestrationRuntimeStatus.Suspended,
            OrchestrationRuntimeStatus.ContinuedAsNew
        });

        do
        {
            var statusQueryResult = await client.ListInstancesAsync(terminateCondition, CancellationToken.None);
            terminateCondition.ContinuationToken = statusQueryResult.ContinuationToken;

            var instancesToTerminate = statusQueryResult.DurableOrchestrationState.Select(o => o.InstanceId).ToHashSet();
            
            await Task.WhenAll(instancesToTerminate.Select(async instanceId => await client.TerminateAsync(instanceId, "Forcibly terminated by overnight clear-down")));
        } while (terminateCondition.ContinuationToken != null);
    }

    private static async Task PurgeOrchestrationsAndDurableEntitiesHistory(IDurableOrchestrationClient client)
    {
        var purgeCondition = CreateOrchestrationQuery(new[]
        {
            OrchestrationRuntimeStatus.Completed,
            OrchestrationRuntimeStatus.Canceled,
            OrchestrationRuntimeStatus.Failed,
            OrchestrationRuntimeStatus.Terminated
        });

        do
        {
            var statusQueryResult = await client.ListInstancesAsync(purgeCondition, CancellationToken.None);
            purgeCondition.ContinuationToken = statusQueryResult.ContinuationToken;

            var instancesToPurge = statusQueryResult.DurableOrchestrationState.Select(o => o.InstanceId).ToHashSet();

            await client.PurgeInstanceHistoryAsync(instancesToPurge);
        } while (purgeCondition.ContinuationToken != null);
    }

    private static OrchestrationStatusQueryCondition CreateOrchestrationQuery(IEnumerable<OrchestrationRuntimeStatus> runtimeStatuses)
    {
        var condition = new OrchestrationStatusQueryCondition
        {
            CreatedTimeFrom = DateTime.MinValue,
            CreatedTimeTo = DateTime.UtcNow,
            RuntimeStatus = runtimeStatuses,
            PageSize = DefaultPageSize,
            ContinuationToken = null
        };
        return condition;
    }
}
