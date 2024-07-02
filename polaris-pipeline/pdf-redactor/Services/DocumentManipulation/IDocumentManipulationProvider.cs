using Common.Dto.Request;

namespace pdf_redactor.Services.DocumentManipulation
{
    public interface IDocumentManipulationProvider
    {
        Task<Stream> ModifyDocument(Stream stream, string caseId, string documentId, ModifyDocumentDto modifyDocument, Guid correlationId);
    }
}