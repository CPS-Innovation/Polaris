using Common.Services.BlobStorage;
using System.IO;
using System.Threading.Tasks;

namespace PolarisGateway.Services.Artefact
{
    public interface ICacheService
    {
        Task<(bool, Stream)> TryGetPdfAsync(int caseId, string documentId, long versionId, bool isOcrProcessed);

        Task UploadPdfAsync(int caseId, string documentId, long versionId, bool isOcrProcessed, Stream stream);

        Task<(bool, T)> TryGetJsonObjectAsync<T>(int caseId, string documentId, long versionId, BlobType blobType);

        Task UploadJsonObjectAsync<T>(int caseId, string documentId, long versionId, BlobType blobType, T obj);
    }
}