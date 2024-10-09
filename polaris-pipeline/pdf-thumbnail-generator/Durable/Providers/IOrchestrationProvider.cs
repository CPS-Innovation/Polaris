
using Common.Dto.Response;
using Microsoft.DurableTask.Client;
using pdf_thumbnail_generator.Domain;
using pdf_thumbnail_generator.Durable.Payloads;

namespace pdf_thumbnail_generator.Durable.Providers
{
  public interface IOrchestrationProvider
  {
    Task<OrchestrationStatus> GenerateThumbnailAsync(DurableTaskClient client, ThumbnailOrchestrationPayload payload);
    Task<List<string>> FindInstancesByDateAsync(DurableTaskClient client, DateTime createdTimeTo, int batchSize);
    Task<DeleteCaseOrchestrationResult> DeleteCaseThumbnailOrchestrationAsync(DurableTaskClient client, string instanceId, DateTime earliestDateToKeep);
  }
}