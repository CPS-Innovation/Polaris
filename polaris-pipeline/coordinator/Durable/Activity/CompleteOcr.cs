using System;
using System.Threading.Tasks;
using Common.Services.BlobStorage;
using Common.Wrappers;
using Common.Services.OcrService;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Common.Domain.Ocr;
using coordinator.Durable.Payloads;
using Common.Configuration;
using Microsoft.Extensions.Configuration;

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

        [FunctionName(nameof(CompleteOcr))]
        public async Task<(bool, AnalyzeResultsStats)> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var (operationId, payload) = context.GetInput<(Guid, DocumentPayload)>();
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
