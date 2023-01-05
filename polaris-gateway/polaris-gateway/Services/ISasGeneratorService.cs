using System;
using System.Threading.Tasks;

namespace PolarisGateway.Services
{
    public interface ISasGeneratorService
    {
        Task<string> GenerateSasUrlAsync(string blobName, Guid correlationId);
    }
}
