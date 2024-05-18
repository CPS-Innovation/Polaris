using System.Threading.Tasks;
using Common.Dto.Response;
using Common.Services.BlobStorageService;
using Common.Telemetry;
using coordinator.Clients.TextExtractor;
using coordinator.Durable.Payloads;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using text_extractor.coordinator;

namespace coordinator.Durable.Activity.ExtractTextNext
{
    public class InitiateIndex
    {
        private readonly IPolarisBlobStorageService _blobStorageService;
        private readonly ITextExtractorClient _textExtractorClient;
        private readonly ITelemetryClient _telemetryClient;

        public InitiateIndex(IPolarisBlobStorageService blobStorageService, ITextExtractorClient textExtractorClient, ITelemetryClient telemetryClient)
        {
            _blobStorageService = blobStorageService;
            _textExtractorClient = textExtractorClient;
            _telemetryClient = telemetryClient;
        }

        [FunctionName(nameof(InitiateIndex))]
        public async Task<StoreCaseIndexesResult> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<CaseDocumentOrchestrationPayload>();
            using var documentStream = await _blobStorageService.GetDocumentAsync(payload.OcrBlobName, payload.CorrelationId);

            var result = await _textExtractorClient.StoreCaseIndexesAsync(
                payload.PolarisDocumentId,
                payload.CmsCaseUrn,
                payload.CmsCaseId,
                payload.CmsDocumentId,
                payload.CmsVersionId,
                payload.BlobName,
                payload.CorrelationId,
                documentStream);

            _telemetryClient.TrackEvent(new VNextDummyEvent(payload.CorrelationId, payload.SubCorrelationId, "InitiateIndex"));

            return result;
        }
    }
}