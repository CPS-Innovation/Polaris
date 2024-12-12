
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Common.Logging;
using pdf_thumbnail_generator.Constants;
using pdf_thumbnail_generator.Services.ClearDownService;
using pdf_thumbnail_generator.Durable.Providers;

namespace pdf_thumbnail_generator.Functions.Maintenance
{
  public class SlidingClearDown
  {
    private readonly ILogger<SlidingClearDown> _logger;
    private readonly IConfiguration _configuration;
    private readonly IClearDownService _clearDownService;
    private readonly IOrchestrationProvider _orchestrationProvider;
    
    public SlidingClearDown(ILogger<SlidingClearDown> logger,
     IConfiguration configuration,
    IClearDownService clearDownService,
    IOrchestrationProvider orchestrationProvider
    )
    {
      _logger = logger;
      _configuration = configuration;
      _clearDownService = clearDownService;
      _orchestrationProvider = orchestrationProvider;
    }

    [Function(nameof(SlidingClearDown))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task Run([TimerTrigger("%SlidingClearDownSchedule%", RunOnStartup = false)] TimerInfo myTimer, [DurableClient] DurableTaskClient client)
    {
      var correlationId = Guid.NewGuid();
      try
      {
        var hoursBack = _configuration[ConfigKeys.SlidingClearDownInputHours] ?? throw new ArgumentNullException($"{ConfigKeys.SlidingClearDownInputHours} cannot be empty");
        var countCases = _configuration[ConfigKeys.SlidingClearDownBatchSize] ?? throw new ArgumentNullException($"{ConfigKeys.SlidingClearDownBatchSize} cannot be empty");
        var hoursBackNumber = double.Parse(hoursBack);
        var countCasesNumber = int.Parse(countCases);
        var earliestDateToKeep = DateTime.UtcNow.AddHours(hoursBackNumber * -1);
        var instanceIds = await _orchestrationProvider.FindInstancesByDateAsync(client, earliestDateToKeep, countCasesNumber);

        foreach (var instanceId in instanceIds)
        {
          // pass an explicit string for the caseUrn for logging purposes as we don't have access to the caseUrn here
          await _clearDownService.DeleteCaseThumbnailAsync(client,
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