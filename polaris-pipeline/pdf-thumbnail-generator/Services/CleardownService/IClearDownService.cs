using Microsoft.DurableTask.Client;

namespace pdf_thumbnail_generator.Services.ClearDownService
{ 
    public interface IClearDownService
    {
        Task DeleteCaseThumbnailAsync(DurableTaskClient client, string caseUrn, string instanceId, DateTime earliestDateToKeep, Guid correlationId);
    }
}
