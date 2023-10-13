using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Common.Domain.BlobStorage;
using Common.ValueObjects;

namespace Common.Services.BlobStorageService.Contracts
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
