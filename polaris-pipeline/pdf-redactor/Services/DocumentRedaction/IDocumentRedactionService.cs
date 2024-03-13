using System;
using System.Threading.Tasks;
using Common.Dto.Request;
using Common.Dto.Response;

namespace pdf_redactor.Services.DocumentRedaction
{
    public interface IDocumentRedactionService
    {
        public Task<RedactPdfResponse> RedactPdfAsync(string caseId, string documentId, RedactPdfRequestDto redactPdfRequest, Guid correlationId);
    }
}
