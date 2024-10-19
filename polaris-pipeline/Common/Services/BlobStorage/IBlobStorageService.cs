using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Common.Services.BlobStorage
{
    public interface IBlobStorageService
    {
        Task UploadBlobAsync(Stream stream, string blobName, IDictionary<string, string> metaData = null);
        Task<Stream> GetBlob(string blobName);
        Task<Stream> TryGetBlobAsync(string blobName, IDictionary<string, string> mustMatchMetadata = null);
        Task<T> TryGetObjectAsync<T>(string blobName);
        Task UploadObjectAsync<T>(T obj, string blobName);
        Task DeleteBlobsByPrefix(string prefix);
        Task<bool> BlobExistsAsync(string blobName, IDictionary<string, string> mustMatchMetadata = null);
    }
}
