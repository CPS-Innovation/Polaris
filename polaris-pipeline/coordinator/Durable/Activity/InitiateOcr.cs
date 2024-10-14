using System;
using System.Threading.Tasks;
using Common.Services.BlobStorageService;
using Common.Services.OcrService;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Durable.Activity
{
    public class InitiateOcr
    {
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IOcrService _ocrService;

        public InitiateOcr(IPolarisBlobStorageService blobStorageService, IOcrService ocrService)
        {
            _blobStorageService = blobStorageService;
            _ocrService = ocrService;
        }

        [FunctionName(nameof(InitiateOcr))]
        public async Task<Guid> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var (blobName, correlationId, _) = context.GetInput<(string, Guid, Guid?)>();
            using var documentStream = await _blobStorageService.GetDocumentAsync(blobName, correlationId);
            return await _ocrService.InitiateOperationAsync(documentStream, correlationId);
        }
    }
}