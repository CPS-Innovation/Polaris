using System;
using System.IO;
using System.Threading.Tasks;

namespace Common.Services.BlobStorageService
{
    public interface IPolarisBlobStorageService
    {
        Task<Stream> GetDocumentAsync(string blobName, Guid correlationId);

        Task UploadDocumentAsync(Stream stream, string blobName);

        Task UploadDocumentAsync(Stream stream, string blobName, int caseId, string documentId, long versionId, Guid correlationId);

        Task DeleteBlobsByCaseAsync(int caseId);
    }
}
