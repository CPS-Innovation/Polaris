using System;
using System.Threading.Tasks;
using Common.Constants;
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
            if (inputConvSucceeded)
            {
                var clearDownPeriod = clearDownInputDays * -1;
                var targetCaseId = await _orchestrationProvider.FindCaseInstanceByDateAsync(DateTime.UtcNow.AddDays(clearDownPeriod), correlationId);

                if (string.IsNullOrEmpty(targetCaseId))
                {
                    _logger.LogMethodFlow(correlationId, nameof(SlidingCaseClearDown), "No candidate case found to clear-down.");
                    return;
                }

                if (!int.TryParse(targetCaseId.Replace("[", "").Replace("]", ""), out var caseId))
                    throw new InvalidCastException($"Invalid case id. A 32-bit integer is expected. A value of {targetCaseId} was found instead");

                _logger.LogMethodFlow(correlationId, nameof(SlidingCaseClearDown), $"Beginning clear down of case {caseId}");
                var deleteResponse = await _orchestrationProvider.DeleteCaseAsync(client, correlationId, caseId, true);
                deleteResponse.EnsureSuccessStatusCode();
                _logger.LogMethodFlow(correlationId, nameof(SlidingCaseClearDown), $"Clear down of case {caseId} completed");
            }
        }
        catch (Exception ex)
        {
            _logger.LogMethodError(correlationId, LoggingName, ex.Message, ex);
        }
    }
}