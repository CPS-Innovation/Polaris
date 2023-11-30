using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Constants;
using Common.Logging;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.Orchestration.Functions.Maintenance;

public class ResetDurableState
{
    private readonly ILogger<ResetDurableState> _logger;
    private readonly IConfiguration _configuration;
    private const string LoggingName = $"{nameof(ResetDurableState)} - {nameof(RunAsync)}";
    private const int DefaultPageSize = 100;
    private const int MaxAzureFunctionRunTimeMinutes = 10;
    // CRON - {second} {minute} {hour} {day} {month} {day-of-week}
    private const string TimerStartTime = "0 0 3 * * *";

    public ResetDurableState(ILogger<ResetDurableState> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [FunctionName(nameof(ResetDurableState))]
    public async Task RunAsync(
        [TimerTrigger(TimerStartTime)] TimerInfo myTimer,
        [DurableClient] IDurableOrchestrationClient client)
    {
        var correlationId = Guid.NewGuid();
        try
        {
            var convSucceeded = bool.TryParse(_configuration[ConfigKeys.CoordinatorKeys.OvernightClearDownEnabled], out var clearDownEnabled);
            if (convSucceeded && clearDownEnabled)
            {
                await TerminateOrchestrationsAndDurableEntities(client, correlationId);
                await WaitForTerminationsToComplete();
                await PurgeOrchestrationsAndDurableEntitiesHistory(client, correlationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogMethodError(correlationId, LoggingName, ex.Message, ex);
        }
    }

    private static async Task WaitForTerminationsToComplete()
    {
        await Task.Delay(TimeSpan.FromMinutes(MaxAzureFunctionRunTimeMinutes + 1));
    }

    private async Task TerminateOrchestrationsAndDurableEntities(IDurableOrchestrationClient client, Guid correlationId)
    {
        var runningInstances = new HashSet<string>();
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
            runningInstances.UnionWith(instancesToTerminate);

            await Task.WhenAll(instancesToTerminate.Select(async instanceId => await client.TerminateAsync(instanceId, "Forcibly terminated by overnight clear-down")));
        } while (terminateCondition.ContinuationToken != null);
    }

    private async Task PurgeOrchestrationsAndDurableEntitiesHistory(IDurableOrchestrationClient client,
        Guid correlationId)
    {
        var orchestrationInstances = new HashSet<string>();
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
            orchestrationInstances.UnionWith(instancesToPurge);

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
