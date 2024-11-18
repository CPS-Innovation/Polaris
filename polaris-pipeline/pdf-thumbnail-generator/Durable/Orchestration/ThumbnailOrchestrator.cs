using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using pdf_thumbnail_generator.Durable.Activity;
using pdf_thumbnail_generator.Durable.Payloads;

namespace pdf_thumbnail_generator.Durable.Orchestration
{
  public class ThumbnailOrchestrator
  {
      public static string GetKey(long caseId, string documentId, long versionId, int maxDimensionPixel)
      {
          return $"[{caseId}]-{documentId}-{versionId}-{maxDimensionPixel}-thumbnail";
      }
      
      [Function(nameof(ThumbnailOrchestrator))]
      public async Task Run([OrchestrationTrigger] TaskOrchestrationContext context)
      {
          var payload = context.GetInput<ThumbnailOrchestrationPayload>() ?? throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));
          
          await context.CallActivityAsync(nameof(InitiateGenerateThumbnail), payload);
      }
  }
}