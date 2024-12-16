using Common.Configuration;
using Common.Services.BlobStorage;
using Microsoft.Extensions.Configuration;

namespace PolarisGateway.Services.Artefact
{
    public class CacheService : ICacheService
    {
        private readonly IPolarisBlobStorageService _polarisBlobStorageService;

        public CacheService(
            Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
            IConfiguration configuration)
        {
            _polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty) ?? throw new ArgumentNullException(nameof(blobStorageServiceFactory));
        }

        public async Task<(bool, Stream)> TryGetPdfAsync(int caseId, string documentId, long versionId, bool isOcrProcessed)
        {
            var blobId = new BlobIdType(caseId, documentId, versionId, BlobType.Pdf);
            var result = await _polarisBlobStorageService.TryGetBlobAsync(blobId, isOcrProcessed);
            return (result != null, result);
        }

        public async Task UploadPdfAsync(int caseId, string documentId, long versionId, bool isOcrProcessed, Stream stream)
        {
            var blobId = new BlobIdType(caseId, documentId, versionId, BlobType.Pdf);
            await _polarisBlobStorageService.UploadBlobAsync(stream, blobId, isOcrProcessed);
        }

        public async Task<(bool, T)> TryGetJsonObjectAsync<T>(int caseId, string documentId, long versionId, BlobType blobType)
        {
            var blobId = new BlobIdType(caseId, documentId, versionId, blobType);
            var result = await _polarisBlobStorageService.TryGetObjectAsync<T>(blobId);
            return (result != null, result);
        }

        public async Task UploadJsonObjectAsync<T>(int caseId, string documentId, long versionId, BlobType blobType, T obj)
        {
            var blobId = new BlobIdType(caseId, documentId, versionId, blobType);
            await _polarisBlobStorageService.UploadObjectAsync(obj, blobId);
        }
    }
}