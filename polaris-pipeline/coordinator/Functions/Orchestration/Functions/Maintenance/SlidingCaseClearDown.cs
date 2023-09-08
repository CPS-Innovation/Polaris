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
    public async Task RunAsync([TimerTrigger("0 */5 * * * *"
            /*, RunOnStartup = true*/
            )]TimerInfo myTimer, [DurableClient] IDurableOrchestrationClient client)
    {
        var correlationId = Guid.NewGuid();
        try
        {
            var convSucceeded = bool.TryParse(_configuration[ConfigKeys.CoordinatorKeys.ClearDownEnabled], out var clearDownEnabled);
            var inputConvSucceeded = short.TryParse(_configuration[ConfigKeys.CoordinatorKeys.ClearDownInputWeeks],
                out var clearDownInputWeeks);
            if (convSucceeded && clearDownEnabled && inputConvSucceeded)
            {
                var clearDownPeriod = clearDownInputWeeks * 7 * -1;
                var targetCaseId =
                    await _orchestrationProvider.FindCaseInstanceByDateAsync(DateTime.UtcNow.AddDays(clearDownPeriod), correlationId);
                if (string.IsNullOrEmpty(targetCaseId))
                {
                    _logger.LogMethodFlow(correlationId, LoggingName, "No cases to clear-down");
                    return;
                }

                if (!int.TryParse(targetCaseId.Replace("[","").Replace("]",""), out var caseId))
                    throw new InvalidCastException(
                        $"Invalid case id. A 32-bit integer is expected. A value of {targetCaseId} was found instead");

                var deleteResponse = await _orchestrationProvider.DeleteCaseAsync(client, correlationId, caseId);
                deleteResponse.EnsureSuccessStatusCode();
            }
            else
            {
                _logger.LogMethodFlow(correlationId, nameof(SlidingCaseClearDown), "Durable data clear-down has been disabled in config.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogMethodError(correlationId, LoggingName, ex.Message, ex);
        }
    }
}