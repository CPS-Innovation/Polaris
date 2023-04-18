using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Logging;
using DurableTask.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.Orchestration.Functions.Maintenance;

public class ResetDurableState
{
    private readonly ILogger<ResetDurableState> _logger;
    private const string LoggingName = $"{nameof(ResetDurableState)} - {nameof(RunAsync)}";

    public ResetDurableState(ILogger<ResetDurableState> logger)
    {
        _logger = logger;
    }
    
    [FunctionName("ResetDurableState")]
    public async Task RunAsync([TimerTrigger("0 0 3 * * *")] TimerInfo myTimer, [DurableClient] IDurableClient client)
    {
        var correlationId = Guid.NewGuid();

        try
        {
            _logger.LogMethodEntry(correlationId, LoggingName, string.Empty);
            
            await client.PurgeInstanceHistoryAsync(
                DateTime.MinValue,
                DateTime.UtcNow,  
                new List<OrchestrationStatus>
                {
                    OrchestrationStatus.Completed,
                    OrchestrationStatus.Failed,
                    OrchestrationStatus.Terminated
                });
        }
        catch (Exception ex)
        {
            _logger.LogMethodError(correlationId, LoggingName, ex.Message, ex);
        }
    }
}
