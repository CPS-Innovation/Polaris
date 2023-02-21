using System;
using System.Threading.Tasks;

namespace PolarisGateway.Services
{
    public interface ISasGeneratorService
    {
        [Obsolete("Moving to to Pipeline")]
        Task<string> GenerateSasUrlAsync(string blobName, Guid correlationId);
    }
}
