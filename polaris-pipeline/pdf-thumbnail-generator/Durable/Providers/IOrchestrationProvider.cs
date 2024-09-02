
using Microsoft.DurableTask.Client;
using pdf_thumbnail_generator.Domain;
using pdf_thumbnail_generator.Durable.Payloads;

namespace pdf_thumbnail_generator.Durable.Providers
{
  public interface IOrchestrationProvider
  {
    Task<OrchestrationStatus> GenerateThumbnailAsync(DurableTaskClient client, ThumbnailOrchestrationPayload payload);
  }
}