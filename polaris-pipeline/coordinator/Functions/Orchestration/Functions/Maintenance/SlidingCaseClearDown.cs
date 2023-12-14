using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Constants;
using Common.Extensions;
using Common.Logging;
using coordinator.Providers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.Orchestration.Functions.Maintenance;

public class SlidingCaseClearDown
{
    private readonly ILogger<SlidingCaseClearDown> _logger;
    private readonly IConfiguration _configuration;
    private readonly IOrchestrationProvider _orchestrationProvider;

    private const string LoggingName = $"{nameof(SlidingCaseClearDown)} - {nameof(RunAsync)}";

    public SlidingCaseClearDown(ILogger<SlidingCaseClearDown> logger, IConfiguration configuration, IOrchestrationProvider orchestrationProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _orchestrationProvider = orchestrationProvider;
    }

    /// <summary>
    /// Cron expression set to run every 5 minutes
    /// </summary>
    /// <param name="myTimer"></param>
    /// <param name="client"></param>
    /// <exception cref="InvalidCastException"></exception>
    [FunctionName(nameof(SlidingCaseClearDown))]
    public async Task RunAsync([TimerTrigger("%SlidingClearDownSchedule%", RunOnStartup = true)]TimerInfo myTimer, [DurableClient] IDurableOrchestrationClient client)
    {
        var correlationId = Guid.NewGuid();
        try
        {
            var inputConvSucceeded = short.TryParse(_configuration[ConfigKeys.CoordinatorKeys.SlidingClearDownInputDays], out var clearDownInputDays);
            var batchConvSucceeded = int.TryParse(_configuration[ConfigKeys.CoordinatorKeys.SlidingClearDownBatchSize], out var clearDownBatchSize);
            if (inputConvSucceeded && batchConvSucceeded)
            {
                var clearDownPeriod = clearDownInputDays * -1;
                var targetCases = await _orchestrationProvider.FindCaseInstancesByDateAsync(DateTime.UtcNow.AddDays(clearDownPeriod), correlationId, clearDownBatchSize);

                if (targetCases.Count == 0)
                {
                    _logger.LogMethodFlow(correlationId, nameof(SlidingCaseClearDown), "No candidate case found to clear-down.");
                    return;
                }

                var tasks = new List<Task<Tuple<int, bool>>>();
                foreach (var targetCaseId in targetCases)
                {
                    if (!int.TryParse(targetCaseId, out var caseId))
                        throw new InvalidCastException($"Invalid case id. A 32-bit integer is expected. A value of {targetCaseId} was found instead");
                    
                    tasks.Add(CallDeleteCaseAsync(correlationId, client, caseId));
                }
                
                foreach (var task in await Task.WhenAll(tasks))
                {
                    _logger.LogMethodFlow(correlationId, nameof(SlidingCaseClearDown), $"Beginning clear down of case {task.Item1}");
                    if (task.Item2)
                    {
                        _logger.LogMethodFlow(correlationId, nameof(SlidingCaseClearDown), $"Clear down of case {task.Item1} completed");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogMethodError(correlationId, LoggingName, ex.Message, ex);
        }
    }
    
    private async Task<Tuple<int, bool>> CallDeleteCaseAsync(Guid correlationId, IDurableOrchestrationClient client, int caseId)
    {
        _logger.LogMethodFlow(correlationId, nameof(SlidingCaseClearDown), $"clearing up {caseId}");
        
        var deleteResponse = await _orchestrationProvider.DeleteCaseAsync(client, correlationId, caseId, true);
        deleteResponse.EnsureSuccessStatusCode();
        
        return Tuple.Create(caseId, true);
    }
}