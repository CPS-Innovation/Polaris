using System.IO;
using System.Threading.Tasks;

namespace Common.Services.BlobStorage
{
    public interface IPolarisBlobStorageService
    {
        Task UploadBlobAsync(Stream stream, BlobIdType blobId);
        Task UploadBlobAsync(Stream stream, BlobIdType blobId, bool ocrFlag);
        Task<Stream> GetBlobAsync(BlobIdType blobId);
        Task<Stream> TryGetBlobAsync(BlobIdType blobId);
        Task<Stream> TryGetBlobAsync(BlobIdType blobId, bool ocrFlagToMatch);
        Task<T> TryGetObjectAsync<T>(BlobIdType blobId);
        Task UploadObjectAsync<T>(T obj, BlobIdType blobId);
        Task DeleteBlobsByPrefixAsync(int prefix);
        Task<bool> BlobExistsAsync(BlobIdType blobId);
        Task<bool> BlobExistsAsync(BlobIdType blobId, bool ocrFlagToMatch);
    }
}
