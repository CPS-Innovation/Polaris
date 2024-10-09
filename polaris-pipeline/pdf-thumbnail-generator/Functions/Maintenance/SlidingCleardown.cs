
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Common.Logging;
using pdf_thumbnail_generator.Constants;
using pdf_thumbnail_generator.Services.CleardownService;
using pdf_thumbnail_generator.Durable.Providers;

namespace pdf_thumbnail_generator.Functions.Maintenance
{
  public class SlidingClearDown
  {
    private readonly ILogger<SlidingClearDown> _logger;
    private readonly IConfiguration _configuration;
    private readonly ICleardownService _cleardownService;
    private readonly IOrchestrationProvider _orchestrationProvider;
    private readonly DurableTaskClient _durableOrchestrationClient;

    public SlidingClearDown(ILogger<SlidingClearDown> logger,
     IConfiguration configuration,
    ICleardownService cleardownService,
    IOrchestrationProvider orchestrationProvider,
    DurableTaskClient durableOrchestrationClient)
    {
      _logger = logger;
      _configuration = configuration;
      _cleardownService = cleardownService;
      _orchestrationProvider = orchestrationProvider;
      _durableOrchestrationClient = durableOrchestrationClient;
    }

    [Function(nameof(SlidingClearDown))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task Run([TimerTrigger("%SlidingClearDownSchedule%", RunOnStartup = true)] TimerInfo myTimer)
    {
      var correlationId = Guid.NewGuid();
      try
      {
        var hoursBack = _configuration[ConfigKeys.SlidingClearDownInputHours] ?? throw new ArgumentNullException($"{ConfigKeys.SlidingClearDownInputHours} cannot be empty");
        var countCases = _configuration[ConfigKeys.SlidingClearDownBatchSize] ?? throw new ArgumentNullException($"{ConfigKeys.SlidingClearDownBatchSize} cannot be empty");
        var hoursBackNumber = double.Parse(hoursBack);
        var countCasesNumber = int.Parse(countCases);
        var earliestDateToKeep = DateTime.UtcNow.AddHours(hoursBackNumber * -1);
        var instanceIds = await _orchestrationProvider.FindInstancesByDateAsync(_durableOrchestrationClient, earliestDateToKeep, countCasesNumber);

        foreach (var instanceId in instanceIds)
        {
          // pass an explicit string for the caseUrn for logging purposes as we don't have access to the caseUrn here
          await _cleardownService.DeleteCaseThumbnailAsync(_durableOrchestrationClient,
           "sliding-clear-down",
           instanceId,
           earliestDateToKeep,
           correlationId);
        }
      }
      catch (Exception ex)
      {
        _logger.LogMethodError(correlationId, nameof(SlidingClearDown), ex.Message, ex);
      }
    }
  }
}