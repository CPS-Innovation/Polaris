using System;
using System.Threading.Tasks;
using PolarisGateway.Domain.DocumentRedaction;

namespace PolarisGateway.Clients.PolarisPipeline
{
    public interface IRedactionClient
    {
        Task<RedactPdfResponse> RedactPdfAsync(RedactPdfRequest redactPdfRequest, string accessToken, Guid correlationId);
    }
}
