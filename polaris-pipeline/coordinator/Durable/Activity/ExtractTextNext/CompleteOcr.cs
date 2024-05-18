using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Common.Services.BlobStorageService;
using Common.Telemetry;
using Common.Wrappers;
using coordinator.Services.OcrService;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using text_extractor.coordinator;

namespace coordinator.Durable.Activity.ExtractTextNext
{
    public class CompleteOcr
    {
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly IOcrService _ocrService;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly ITelemetryClient _telemetryClient;

        public CompleteOcr(IPolarisBlobStorageService blobStorageService, IOcrService ocrService, IJsonConvertWrapper jsonConvertWrapper, ITelemetryClient telemetryClient)
        {
            _blobStorageService = blobStorageService;
            _ocrService = ocrService;
            _jsonConvertWrapper = jsonConvertWrapper;
            _telemetryClient = telemetryClient;
        }

        [FunctionName(nameof(CompleteOcr))]
        public async Task<bool> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var (operationId, ocrBlobName, correlationId, subCorrelationId) = context.GetInput<(Guid, string, Guid, Guid?)>();
            var (isOperationComplete, operationResults) = await _ocrService.GetOperationResultsAsync(operationId, correlationId);

            if (!isOperationComplete)
            {
                return false;
            }

            var jsonResults = _jsonConvertWrapper.SerializeObject(operationResults.AnalyzeResult);
            using var ocrStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonResults));

            await _blobStorageService.UploadDocumentAsync(
                ocrStream,
                ocrBlobName);

            _telemetryClient.TrackEvent(new VNextDummyEvent(correlationId, subCorrelationId, "CompleteOcr"));

            return true;
        }
    }
}
