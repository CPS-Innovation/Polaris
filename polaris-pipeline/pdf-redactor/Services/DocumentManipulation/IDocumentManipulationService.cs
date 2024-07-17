using Common.Dto.Request;

namespace pdf_redactor.Services.DocumentManipulation
{
    public interface IDocumentManipulationService
    {
        public Task<Stream> RemoveOrRotatePagesAsync(string caseId, string documentId, ModifyDocumentWithDocumentDto removeOrRotateDocumentPages, Guid correlationId);
    }
}