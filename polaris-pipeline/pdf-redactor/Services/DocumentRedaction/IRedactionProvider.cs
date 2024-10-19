using Common.Dto.Request;

namespace pdf_redactor.Services.DocumentRedaction
{
    public interface IRedactionProvider
    {
        Task<Stream> Redact(Stream stream, int caseId, string documentId, RedactPdfRequestDto redactPdfRequest, Guid correlationId);
    }
}