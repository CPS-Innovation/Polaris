using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Common.Services.BlobStorageService
{
    public interface IPolarisBlobStorageService
    {
        Task UploadBlobAsync(Stream stream, string blobName);
        Task UploadBlobAsync(Stream stream, string blobName, IDictionary<string, string> metaData);
        Task<Stream> GetBlobOrThrowAsync(string blobName);
        Task<Stream> GetBlobAsync(string blobName);
        Task<Stream> GetBlobAsync(string blobName, IDictionary<string, string> mustMatchMetadata);
        Task<T> GetJsonBlobAsync<T>(string blobName);
        Task DeleteBlobsByCaseAsync(int caseId);
    }
}
