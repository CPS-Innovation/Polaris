using Microsoft.DurableTask.Client;


namespace pdf_thumbnail_generator.Services.CleardownService
{
  public interface ICleardownService
  {
    Task DeleteCaseThumbnailAsync(DurableTaskClient client, string caseUrn, string instanceId, DateTime earliestDateToKeep, Guid correlationId);
  }
}
