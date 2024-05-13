using System;
using System.Text;
using System.Threading.Tasks;
using Common.Services.BlobStorageService;
using Common.Telemetry;
using coordinator.Durable.Payloads;
using coordinator.Services.OcrService;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using text_extractor.coordinator;

namespace coordinator.Durable.Activity
{
    public class PollOcr
    {
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IOcrService _ocrService;
        private readonly ITelemetryClient _telemetryClient;

        public PollOcr(IPolarisBlobStorageService blobStorageService, IOcrService ocrService, ITelemetryClient telemetryClient)
        {
            _blobStorageService = blobStorageService;
            _ocrService = ocrService;
            _telemetryClient = telemetryClient;
        }

        [FunctionName(nameof(PollOcr))]
        public async Task<ReadOperationResult> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var (operationId, correlationId) = context.GetInput<(Guid, Guid)>();

            return await _ocrService.GetOperationResultsAsync(operationId, correlationId);

        }
    }
}
