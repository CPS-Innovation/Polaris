
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using pdf_thumbnail_generator.Durable.Activity;
using pdf_thumbnail_generator.Durable.Payloads;

namespace pdf_thumbnail_generator.Durable.Orchestration
{
  public class ThumbnailOrchestrator
  {
    private readonly ILogger<ThumbnailOrchestrator> _logger;
    public static string GetKey(long caseId, string documentId, int versionId)
    {
      return $"[{caseId}]-{documentId}-{versionId}-thumbnail";
    }

    public ThumbnailOrchestrator(ILogger<ThumbnailOrchestrator> logger)
    {
      _logger = logger;
    }

    [Function(nameof(ThumbnailOrchestrator))]
    public async Task Run([OrchestrationTrigger] TaskOrchestrationContext context)
    {
      var payload = context.GetInput<ThumbnailOrchestrationPayload>()
          ?? throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));


      await context.CallActivityAsync(nameof(InitiateGenerateThumbnail), payload);

    }
  }
}