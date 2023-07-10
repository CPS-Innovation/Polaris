using System;
using System.IO;
using System.Threading.Tasks;
using Common.ValueObjects;

namespace Common.Clients.Contracts
{
    public interface ITextExtractorClient
    {
        Task ExtractTextAsync(PolarisDocumentId polarisDocumentId, long cmsCaseId, string cmsDocumentId, long versionId, string blobName, Guid correlationId, Stream documentStream);
        Task<string> SearchTextAsync();
    }
}