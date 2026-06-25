using Common.Configuration;
using Common.Services.BlobStorage;
using Common.Services.BlobStorage.Factories;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

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

        public async Task<(bool, Stream)> TryGetPdfAsync(int caseId, string materialId, long documentId, bool isOcrProcessed)
        {
            var blobId = _blobTypeIdFactory.CreateBlobId(caseId, materialId, documentId, BlobType.Pdf);
            var result = await _polarisBlobStorageService.TryGetBlobAsync(blobId, isOcrProcessed);
            return (result != null, result);
        }

        public async Task UploadPdfAsync(int caseId, string materialId, long documentId, bool isOcrProcessed, Stream stream, double fileSizeInMb)
        {
            var blobId = _blobTypeIdFactory.CreateBlobId(caseId, materialId, documentId, BlobType.Pdf);
            await _polarisBlobStorageService.UploadBlobAsync(stream, blobId, isOcrProcessed, fileSizeInMb);
        }

        public async Task<(bool, T)> TryGetJsonObjectAsync<T>(int caseId, string materialId, long documentId, BlobType blobType)
        {
            var blobId = _blobTypeIdFactory.CreateBlobId(caseId, materialId, documentId, blobType);
            var result = await _polarisBlobStorageService.TryGetObjectAsync<T>(blobId);
            return (result != null, result);
        }

        public async Task UploadJsonObjectAsync<T>(int caseId, string materialId, long documentId, BlobType blobType, T obj)
        {
            var blobId = _blobTypeIdFactory.CreateBlobId(caseId, materialId, documentId, blobType);
            await _polarisBlobStorageService.UploadObjectAsync(obj, blobId);
        }

        public Task SetPdfSizeAsync(int caseId, string materialId, long documentId, bool isOcrProcessed, double fileSizeInMb)
        {
            var key = $"pdf-size-{caseId}-{materialId}-{documentId}-{isOcrProcessed}";
            return _polarisBlobStorageService.UploadSizeAsync(key, fileSizeInMb);
        }

        public async Task<double?> GetPdfSizeFromMetadataAsync(int caseId, string materialId, long documentId, bool isOcrProcessed)
        {
            var blobId = _blobTypeIdFactory.CreateBlobId(
                caseId,
                materialId,
                documentId,
                BlobType.Pdf);

            var metadata = await _polarisBlobStorageService.GetMetadataAsync(blobId);

            if (metadata != null &&
                metadata.TryGetValue("FileSizeInMb", out var value) &&
                double.TryParse(value, out var size))
            {
                return size;
            }

            return null;
        }
    }
}