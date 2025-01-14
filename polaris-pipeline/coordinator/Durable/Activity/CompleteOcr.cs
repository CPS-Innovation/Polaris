using System;
using System.Threading.Tasks;
using Common.Services.BlobStorage;
using Common.Services.OcrService;
using Common.Domain.Ocr;
using Common.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Worker;
using coordinator.Domain;
using coordinator.Durable.Orchestration;

namespace coordinator.Durable.Activity
{
    public class CompleteOcr
    {
        private readonly IPolarisBlobStorageService _polarisBlobStorageService;
        private readonly IOcrService _ocrService;

        public CompleteOcr(Func<string, IPolarisBlobStorageService> blobStorageServiceFactory, IOcrService ocrService, IConfiguration configuration)
        {
            _polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty) ?? throw new ArgumentNullException(nameof(blobStorageServiceFactory));
            _ocrService = ocrService;
        }

        [Function(nameof(CompleteOcr))]
        public async Task<PollingActivityResult<CompleteOcrResponse>> Run([ActivityTrigger] CompleteOcrPayload completeOcrPayload)
        {
            var ocrOperationResult = await _ocrService.GetOperationResultsAsync(completeOcrPayload.OcrOperationId, completeOcrPayload.Payload.CorrelationId);

            if (!ocrOperationResult.IsSuccess)
            {
                return new PollingActivityResult<CompleteOcrResponse>
                {
                    Result = new CompleteOcrResponse { BlobAlreadyExists = false },
                    IsCompleted = false,
                };
            }

            var blobId = new BlobIdType(completeOcrPayload.Payload.CaseId, completeOcrPayload.Payload.DocumentId, completeOcrPayload.Payload.VersionId, BlobType.Ocr);
            await _polarisBlobStorageService.UploadObjectAsync(ocrOperationResult.AnalyzeResults, blobId);

            return new PollingActivityResult<CompleteOcrResponse>
            {
                Result = new CompleteOcrResponse { BlobAlreadyExists = true, OcrResult = AnalyzeResultsStats.FromAnalyzeResults(ocrOperationResult.AnalyzeResults) },
                IsCompleted = true
            };
        }
    }
}