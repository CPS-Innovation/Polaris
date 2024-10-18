using System;
using System.Threading.Tasks;
using Common.Services.BlobStorage;
using Common.Services.OcrService;
using coordinator.Durable.Payloads;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Durable.Activity
{
    public class InitiateOcr
    {
        private readonly IPolarisBlobStorageService _polarisBlobStorageService;
        private readonly IOcrService _ocrService;

        public InitiateOcr(IPolarisBlobStorageService polarisBlobStorageService, IOcrService ocrService)
        {
            _polarisBlobStorageService = polarisBlobStorageService;
            _ocrService = ocrService;
        }

        [FunctionName(nameof(InitiateOcr))]

        public async Task<Guid> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<DocumentPayload>();
            var blobId = new BlobIdType(payload.CaseId, payload.DocumentId, payload.VersionId, BlobType.Pdf);

            using var documentStream = await _polarisBlobStorageService.GetBlobAsync(blobId);
            return await _ocrService.InitiateOperationAsync(documentStream, payload.CorrelationId);
        }
    }
}