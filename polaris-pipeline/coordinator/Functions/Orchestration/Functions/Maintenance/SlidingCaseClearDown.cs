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

    public SlidingCaseClearDown(ILogger<SlidingCaseClearDown> logger, IConfiguration configuration, IOrchestrationProvider orchestrationProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _orchestrationProvider = orchestrationProvider;
    }

    [FunctionName(nameof(SlidingCaseClearDown))]
    public async Task RunAsync([TimerTrigger("%SlidingClearDownSchedule%")] TimerInfo myTimer, [DurableClient] IDurableOrchestrationClient client)
    {
        var correlationId = Guid.NewGuid();
        try
        {
            var daysBackNumber = short.Parse(_configuration[ConfigKeys.CoordinatorKeys.SlidingClearDownInputDays]);
            var countCases = int.Parse(_configuration[ConfigKeys.CoordinatorKeys.SlidingClearDownBatchSize]);
            var earliestDateToKeep = DateTime.UtcNow.AddDays(daysBackNumber * -1);
            var caseIds = await _orchestrationProvider.FindCaseInstancesByDateAsync(client, earliestDateToKeep, countCases);

            // first pass: lets do the cases in sequence rather than parallel, until we are sure of search index characteristics
            foreach (var caseId in caseIds)
            {
                await _orchestrationProvider.DeleteCaseAsync(client,
                 correlationId,
                 caseId,
                 checkForBlobProtection: true,
                 waitForIndexToSettle: false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogMethodError(correlationId, nameof(SlidingCaseClearDown), ex.Message, ex);
        }
    }
}