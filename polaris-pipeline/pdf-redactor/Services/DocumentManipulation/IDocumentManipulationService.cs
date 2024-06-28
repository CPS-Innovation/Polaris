using Common.Dto.Request;

namespace pdf_redactor.Services.DocumentManipulation
{
    public interface IDocumentManipulationService
    {
        public Task<Stream> RemovePagesAsync(string caseId, string documentId, RemoveDocumentPagesWithDocumentDto removeDocumentPages, Guid correlationId);
    }
}