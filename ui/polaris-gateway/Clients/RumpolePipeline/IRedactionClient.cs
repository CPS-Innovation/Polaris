using System;
using System.Threading.Tasks;
using RumpoleGateway.Domain.DocumentRedaction;

namespace RumpoleGateway.Clients.RumpolePipeline
{
    public interface IRedactionClient
    {
        Task<RedactPdfResponse> RedactPdfAsync(RedactPdfRequest redactPdfRequest, string accessToken, Guid correlationId);
    }
}
