using System.IO;
using System.Threading.Tasks;

namespace Common.Services.BlobStorage
{
    public interface IPolarisBlobStorageService
    {
        Task UploadBlobAsync(Stream stream, BlobIdType blobId, bool? isOcred = null, System.Threading.CancellationToken cancellationToken = default);

        Task UploadBlobAsync(Stream stream, BlobIdType blobId, int? pageIndex = null, int? maxDimensionPixel = null, System.Threading.CancellationToken cancellationToken = default);

        Task<Stream> GetBlobAsync(BlobIdType blobId);

        Task<Stream> TryGetBlobAsync(BlobIdType blobId, bool? mustBeOcred = null, System.Threading.CancellationToken cancellationToken = default);

        Task<T> TryGetObjectAsync<T>(BlobIdType blobId, System.Threading.CancellationToken cancellationToken = default);

        Task UploadObjectAsync<T>(T obj, BlobIdType blobId, System.Threading.CancellationToken cancellationToken = default);

        Task DeleteBlobsByPrefixAsync(int prefix, System.Threading.CancellationToken cancellationToken = default);

        Task<bool> BlobExistsAsync(BlobIdType blobId, bool? mustBeOcred = null, System.Threading.CancellationToken cancellationToken = default);
    }
}
