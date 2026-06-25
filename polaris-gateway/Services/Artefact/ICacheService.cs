using Common.Services.BlobStorage;
using System.IO;
using System.Threading.Tasks;

namespace PolarisGateway.Services.Artefact
{
    public interface ICacheService
    {
        Task<(bool, Stream)> TryGetPdfAsync(int caseId, string materialId, long documentId, bool isOcrProcessed);

        Task UploadPdfAsync(int caseId, string materialId, long documentId, bool isOcrProcessed, Stream stream, double fileSizeInMb);

        Task<(bool, T)> TryGetJsonObjectAsync<T>(int caseId, string materialId, long documentId, BlobType blobType);

        Task UploadJsonObjectAsync<T>(int caseId, string materialId, long documentId, BlobType blobType, T obj);

        Task SetPdfSizeAsync(int caseId, string materialId, long documentId, bool isOcrProcessed, double fileSizeInMb);

        Task<double?> GetPdfSizeFromMetadataAsync(int caseId, string materialId, long documentId, bool isOcrProcessed);
    }
}