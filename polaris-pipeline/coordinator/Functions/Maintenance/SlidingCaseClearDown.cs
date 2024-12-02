using System;
using System.Threading.Tasks;
using Common.Logging;
using coordinator.Constants;
using coordinator.Durable.Providers;
using coordinator.Services.ClearDownService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.Maintenance;

public class SlidingCaseClearDown
{
    private readonly ILogger<SlidingCaseClearDown> _logger;
    private readonly IConfiguration _configuration;
    private readonly IOrchestrationProvider _orchestrationProvider;
    private readonly IClearDownService _clearDownService;

    public SlidingCaseClearDown(ILogger<SlidingCaseClearDown> logger, IConfiguration configuration, IOrchestrationProvider orchestrationProvider, IClearDownService clearDownService)
    {
        _logger = logger;
        _configuration = configuration;
        _orchestrationProvider = orchestrationProvider;
        _clearDownService = clearDownService;
    }

    [Function(nameof(SlidingCaseClearDown))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task RunAsync([TimerTrigger("%SlidingClearDownSchedule%")] TimerInfo myTimer, [DurableClient] DurableTaskClient client)
    {
        var correlationId = Guid.NewGuid();
        try
        {
            var hoursBackNumber = double.Parse(_configuration[ConfigKeys.SlidingClearDownInputHours]);
            var countCases = int.Parse(_configuration[ConfigKeys.SlidingClearDownBatchSize]);
            var earliestDateToKeep = DateTime.UtcNow.AddHours(hoursBackNumber * -1);
            var caseIds = await _orchestrationProvider.FindCaseInstancesByDateAsync(client, earliestDateToKeep, countCases);

            // first pass: lets do the cases in sequence rather than parallel, until we are sure of search index characteristics
            foreach (var caseId in caseIds)
            {
                // pass an explicit string for the caseUrn for logging purposes as we don't have access to the caseUrn here
                await _clearDownService.DeleteCaseAsync(client,
                 "sliding-clear-down",
                 caseId,
                 correlationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogMethodError(correlationId, nameof(SlidingCaseClearDown), ex.Message, ex);
        }
    }
}