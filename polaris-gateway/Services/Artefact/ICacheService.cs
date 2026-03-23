using Common.Services.BlobStorage;
using System.IO;
using System.Threading.Tasks;

namespace PolarisGateway.Services.Artefact
{
    public interface ICacheService
    {
        Task<(bool, Stream)> TryGetPdfAsync(int caseId, string documentId, long versionId, bool isOcrProcessed, System.Threading.CancellationToken cancellationToken = default);

        Task UploadPdfAsync(int caseId, string documentId, long versionId, bool isOcrProcessed, Stream stream, System.Threading.CancellationToken cancellationToken = default);

        Task<(bool, T)> TryGetJsonObjectAsync<T>(int caseId, string documentId, long versionId, BlobType blobType, System.Threading.CancellationToken cancellationToken = default);

        Task UploadJsonObjectAsync<T>(int caseId, string documentId, long versionId, BlobType blobType, T obj, System.Threading.CancellationToken cancellationToken = default);
    }
}