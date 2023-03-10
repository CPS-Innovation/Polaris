using System;
using System.Threading.Tasks;

namespace Common.Services.SasGeneratorService
{
    public interface ISasGeneratorService
    {
        Task<string> GenerateSasUrlAsync(string blobName, Guid correlationId);
    }
}
