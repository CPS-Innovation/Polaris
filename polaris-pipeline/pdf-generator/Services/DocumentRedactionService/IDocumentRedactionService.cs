using System;
using System.Threading.Tasks;
using Common.Domain.Requests;
using Common.Domain.Responses;

namespace pdf_generator.Services.DocumentRedactionService
{
    public interface IDocumentRedactionService
    {
        public Task<RedactPdfResponse> RedactPdfAsync(RedactPdfRequest redactPdfRequest, string accessToken, Guid correlationId);
    }
}
