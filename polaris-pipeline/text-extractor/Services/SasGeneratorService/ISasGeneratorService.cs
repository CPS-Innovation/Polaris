using System;
using System.Threading.Tasks;

namespace text_extractor.Services.SasGeneratorService
{
    public interface ISasGeneratorService
    {
        Task<string> GenerateSasUrlAsync(string blobName, Guid correlationId);
    }
}
