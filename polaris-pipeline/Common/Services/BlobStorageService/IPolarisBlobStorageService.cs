using System;
using System.IO;
using System.Threading.Tasks;
using Common.ValueObjects;

namespace Common.Services.BlobStorageService
{
    public interface IPolarisBlobStorageService
    {
        Task<Stream> GetDocumentAsync(string blobName, Guid correlationId);
        Task<Stream> GetDocumentVersionAsync(string blobName, string versionId);
        Task UploadDocumentAsync(Stream stream, string blobName);

        Task UploadDocumentAsync(Stream stream, string blobName, string caseId, PolarisDocumentId polarisDocumentId, string versionId, Guid correlationId);

        Task DeleteBlobsByCaseAsync(string caseId);
    }
}
