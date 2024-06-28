using Common.Dto.Request;

namespace pdf_redactor.Services.DocumentManipulation
{
    public interface IDocumentManipulationProvider
    {
        Task<Stream> RemovePages(Stream stream, string caseId, string documentId, RemoveDocumentPagesDto removeDocumentPages, Guid correlationId);
    }
}