using Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using pdf_thumbnail_generator.Durable.Payloads;
using pdf_thumbnail_generator.Durable.Providers;
using Common.Extensions;
using pdf_thumbnail_generator.Domain;
using Common.Handlers;

namespace pdf_thumbnail_generator.Functions
{
  public class GenerateThumbnail
  {
    private readonly ILogger<GenerateThumbnail> _logger;
    private readonly DurableTaskClient _durableOrchestrationClient;
    private readonly IOrchestrationProvider _orchestrationProvider;
    private readonly IExceptionHandler _exceptionHandler;

    public GenerateThumbnail(
      ILogger<GenerateThumbnail> logger,
      DurableTaskClient durableOrchestrationClient,
      IOrchestrationProvider orchestrationProvider,
      IExceptionHandler exceptionHandler)
    {
      _logger = logger;
      _durableOrchestrationClient = durableOrchestrationClient;
      _orchestrationProvider = orchestrationProvider;
      _exceptionHandler = exceptionHandler;
    }

    [Function(nameof(GenerateThumbnail))]
    public async Task<IActionResult> Run(
      [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.GenerateThumbnail)] HttpRequest req,
      string caseUrn,
      int caseId,
      string documentId,
      int versionId,
      int? pageNumber = null
      )
    {
      Guid currentCorrelationId = default;
      try
      {
        currentCorrelationId = req.Headers.GetCorrelationId();

        var payload = new ThumbnailOrchestrationPayload(caseUrn, caseId, documentId, versionId, currentCorrelationId, pageNumber);
        var result = await _orchestrationProvider.GenerateThumbnailAsync(_durableOrchestrationClient, payload);

        return result switch
        {
          OrchestrationStatus.Accepted or OrchestrationStatus.Completed => new ObjectResult(new ThumbnailResponse(caseUrn, caseId, documentId, versionId))
          {
            StatusCode = StatusCodes.Status202Accepted
          },
          OrchestrationStatus.InProgress => new StatusCodeResult(StatusCodes.Status423Locked),
          _ => new StatusCodeResult(StatusCodes.Status500InternalServerError),
        };
      }
      catch (Exception ex)
      {
        return _exceptionHandler.HandleExceptionNew(ex, currentCorrelationId, nameof(GenerateThumbnail), _logger);
      }
    }
  }
}