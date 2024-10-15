using Common.Dto.Request;

namespace pdf_redactor.Services.DocumentManipulation
{
    public interface IDocumentManipulationProvider
    {
        Task<Stream> ModifyDocument(Stream stream, int caseId, string documentId, ModifyDocumentDto modifyDocument, Guid correlationId);
    }
}