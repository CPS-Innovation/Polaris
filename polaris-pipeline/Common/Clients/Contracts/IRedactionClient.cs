using Common.Domain.Requests;
using Common.Domain.Responses;
using System;
using System.Threading.Tasks;

namespace Common.Clients.Contracts
{
    public interface IRedactionClient
    {
        Task<RedactPdfResponse> RedactPdfAsync(RedactPdfRequest redactPdfRequest, Guid correlationId);
    }
}
