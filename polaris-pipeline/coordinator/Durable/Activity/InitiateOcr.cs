using System;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Services.BlobStorage;
using Common.Services.OcrService;
using coordinator.Durable.Payloads;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;

namespace coordinator.Durable.Activity
{
    public class InitiateOcr
    {
        private readonly IPolarisBlobStorageService _polarisBlobStorageService;
        private readonly IOcrService _ocrService;

        public InitiateOcr(Func<string, IPolarisBlobStorageService> blobStorageServiceFactory, IOcrService ocrService, IConfiguration configuration)
        {
            _polarisBlobStorageService = blobStorageServiceFactory(configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty) ?? throw new ArgumentNullException(nameof(blobStorageServiceFactory));
            _ocrService = ocrService;
        }

        [FunctionName(nameof(InitiateOcr))]

        public async Task<(bool, Guid)> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<DocumentPayload>();
            var ocrBlobId = new BlobIdType(payload.CaseId, payload.DocumentId, payload.VersionId, BlobType.Ocr);
            if (await _polarisBlobStorageService.BlobExistsAsync(ocrBlobId))
            {
                return (true, Guid.Empty);
            }

            var pdfBlobId = new BlobIdType(payload.CaseId, payload.DocumentId, payload.VersionId, BlobType.Pdf);
            await using var documentStream = await _polarisBlobStorageService.GetBlobAsync(pdfBlobId);
            return (false, await _ocrService.InitiateOperationAsync(documentStream, payload.CorrelationId));
        }
    }
}