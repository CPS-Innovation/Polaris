using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Common.Services.BlobStorage
{
    public interface IBlobStorageService
    {
        Task UploadBlobAsync(Stream stream, string blobName, IDictionary<string, string> metaData = null, System.Threading.CancellationToken cancellationToken = default);
        Task<Stream> GetBlob(string blobName);
        Task<Stream> TryGetBlobAsync(string blobName, IDictionary<string, string> mustMatchMetadata = null, System.Threading.CancellationToken cancellationToken = default);
        Task<T> TryGetObjectAsync<T>(string blobName, System.Threading.CancellationToken cancellationToken = default);
        Task<bool> UploadObjectAsync<T>(T obj, string blobName, System.Threading.CancellationToken cancellationToken = default);
        Task DeleteBlobsByPrefix(string prefix, System.Threading.CancellationToken cancellationToken = default);
        Task<bool> BlobExistsAsync(string blobName, IDictionary<string, string> mustMatchMetadata = null, System.Threading.CancellationToken cancellationToken = default);
    }
}
