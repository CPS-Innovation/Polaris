using Common.Dto.Request;
using Common.Dto.Response;
using System;
using System.Threading.Tasks;

namespace Common.Clients.Contracts
{
    public interface IRedactionClient
    {
        Task<RedactPdfResponse> RedactPdfAsync(RedactPdfRequestDto redactPdfRequest, Guid correlationId);
    }
}
