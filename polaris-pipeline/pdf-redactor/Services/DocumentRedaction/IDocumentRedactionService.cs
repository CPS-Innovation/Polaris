using Common.Dto.Request;

namespace pdf_redactor.Services.DocumentRedaction
{
    public interface IDocumentRedactionService
    {
        public Task<Stream> RedactAsync(int caseId, string documentId, RedactPdfRequestWithDocumentDto redactPdfRequest, Guid correlationId);

    }
}
