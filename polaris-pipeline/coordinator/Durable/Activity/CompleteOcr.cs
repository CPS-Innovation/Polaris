using System;
using System.Threading.Tasks;
using Common.Services.BlobStorage;
using Common.Wrappers;
using Common.Services.OcrService;
using Common.Domain.Ocr;
using coordinator.Durable.Payloads;
using Common.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Worker;

namespace coordinator.Durable.Activity
{
    public class CompleteOcr
    {
        private readonly IPolarisBlobStorageService _polarisBlobStorageService;
        private readonly IOcrService _ocrService;

        public CompleteOcr(Func<string, IPolarisBlobStorageService> blobStorageServiceFactory, IOcrService ocrService, IJsonConvertWrapper jsonConvertWrapper, IConfiguration configuration)
        {
            _polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty) ?? throw new ArgumentNullException(nameof(blobStorageServiceFactory));
            _ocrService = ocrService;
        }

        [Function(nameof(CompleteOcr))]
        public async Task<(bool, AnalyzeResultsStats)> Run([ActivityTrigger] Guid operationId, DocumentPayload payload)
        {
            var ocrOperationResult = await _ocrService.GetOperationResultsAsync(operationId, payload.CorrelationId);

            if (!ocrOperationResult.IsSuccess)
            {
                return (false, null);
            }

            var blobId = new BlobIdType(payload.CaseId, payload.DocumentId, payload.VersionId, BlobType.Ocr);
            await _polarisBlobStorageService.UploadObjectAsync(ocrOperationResult.AnalyzeResults, blobId);

            return (true, AnalyzeResultsStats.FromAnalyzeResults(ocrOperationResult.AnalyzeResults));
        }
    }
}
