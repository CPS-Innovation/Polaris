using System;
using System.Threading.Tasks;
using polaris_common.Dto.Request;
using polaris_common.Dto.Response;

namespace pdf_generator.Services.DocumentRedaction
{
    public interface IDocumentRedactionService
    {
        public Task<RedactPdfResponse> RedactPdfAsync(RedactPdfRequestDto redactPdfRequest, Guid correlationId);
    }
}
