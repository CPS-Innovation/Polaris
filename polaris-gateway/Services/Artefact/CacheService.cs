using Common.Configuration;
using Common.Services.BlobStorage;
using Common.Services.BlobStorage.Factories;
using Microsoft.Extensions.Configuration;

namespace PolarisGateway.Services.Artefact
{
    public class CacheService : ICacheService
    {
        private readonly IPolarisBlobStorageService _polarisBlobStorageService;
        private readonly IBlobTypeIdFactory _blobTypeIdFactory;

        public CacheService(
            Func<string, IPolarisBlobStorageService> blobStorageServiceFactory,
            IConfiguration configuration,
            IBlobTypeIdFactory blobTypeIdFactory)
        {
            _polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty) ?? throw new ArgumentNullException(nameof(blobStorageServiceFactory));
            _blobTypeIdFactory = blobTypeIdFactory ?? throw new ArgumentNullException(nameof(blobTypeIdFactory));
        }

        public async Task<(bool, Stream)> TryGetPdfAsync(int caseId, string documentId, long versionId, bool isOcrProcessed)
        {
            var blobId = _blobTypeIdFactory.CreateBlobId(caseId, documentId, versionId, BlobType.Pdf);
            var result = await _polarisBlobStorageService.TryGetBlobAsync(blobId, isOcrProcessed);
            return (result != null, result);
        }

        public async Task UploadPdfAsync(int caseId, string documentId, long versionId, bool isOcrProcessed, Stream stream)
        {
            var blobId = _blobTypeIdFactory.CreateBlobId(caseId, documentId, versionId, BlobType.Pdf);
            await _polarisBlobStorageService.UploadBlobAsync(stream, blobId, isOcrProcessed);
        }

        public async Task<(bool, T)> TryGetJsonObjectAsync<T>(int caseId, string documentId, long versionId, BlobType blobType)
        {
            var blobId = _blobTypeIdFactory.CreateBlobId(caseId, documentId, versionId, blobType);
            var result = await _polarisBlobStorageService.TryGetObjectAsync<T>(blobId);
            return (result != null, result);
        }

        public async Task UploadJsonObjectAsync<T>(int caseId, string documentId, long versionId, BlobType blobType, T obj)
        {
            var blobId = _blobTypeIdFactory.CreateBlobId(caseId, documentId, versionId, blobType);
            await _polarisBlobStorageService.UploadObjectAsync(obj, blobId);
        }
    }
}