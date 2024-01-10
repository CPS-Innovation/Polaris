using polaris_common.Domain.BlobStorage;
using polaris_common.ValueObjects;

namespace polaris_common.Services.BlobStorageService.Contracts
{
    public interface IPolarisBlobStorageService
    {
        Task<bool> DocumentExistsAsync(string blobName, Guid correlationId);

        Task<List<BlobSearchResult>> FindBlobsByPrefixAsync(string blobPrefix, Guid correlationId);
        
        Task<Stream> GetDocumentAsync(string blobName, Guid correlationId);

        Task UploadDocumentAsync(Stream stream, string blobName, string caseId, PolarisDocumentId polarisDocumentId, string versionId, Guid correlationId);

        Task<bool> RemoveDocumentAsync(string blobName, Guid correlationId);
        
        Task DeleteBlobsByCaseAsync(string caseId, Guid correlationId);
    }
}
