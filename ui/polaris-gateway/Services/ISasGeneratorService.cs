using System;
using System.Threading.Tasks;

namespace RumpoleGateway.Services
{
    public interface ISasGeneratorService
    {
        Task<string> GenerateSasUrlAsync(string blobName, Guid correlationId);
    }
}
