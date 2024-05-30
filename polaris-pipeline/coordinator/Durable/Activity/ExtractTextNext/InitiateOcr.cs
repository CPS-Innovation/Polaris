using System;
using System.Threading.Tasks;
using Common.Services.BlobStorageService;
using Common.Telemetry;
using coordinator.Services.OcrService;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using text_extractor.coordinator;

namespace coordinator.Durable.Activity.ExtractTextNext
{
    public class InitiateOcr
    {
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IOcrService _ocrService;
        private readonly ITelemetryClient _telemetryClient;

        public InitiateOcr(IPolarisBlobStorageService blobStorageService, IOcrService ocrService, ITelemetryClient telemetryClient)
        {
            _blobStorageService = blobStorageService;
            _ocrService = ocrService;
            _telemetryClient = telemetryClient;
        }

        [FunctionName(nameof(InitiateOcr))]
        public async Task<Guid> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var (blobName, correlationId, subCorrelationId) = context.GetInput<(string, Guid, Guid?)>();
            using var documentStream = await _blobStorageService.GetDocumentAsync(blobName, correlationId);
            var operationId = await _ocrService.InitiateOperationAsync(documentStream, correlationId);

            _telemetryClient.TrackEvent(new VNextDummyEvent(correlationId, subCorrelationId, "InitiateOcr"));

            return operationId;
        }
    }
}