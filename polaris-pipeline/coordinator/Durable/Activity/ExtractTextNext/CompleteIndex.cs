using System.Threading.Tasks;
using Common.Telemetry;
using coordinator.Clients.TextExtractor;
using coordinator.Durable.Payloads;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using text_extractor.coordinator;

namespace coordinator.Durable.Activity.ExtractTextNext
{
    public class CompleteIndex
    {
        private readonly ITextExtractorClient _textExtractorClient;
        private readonly ITelemetryClient _telemetryClient;

        public CompleteIndex(ITextExtractorClient textExtractorClient, ITelemetryClient telemetryClient)
        {
            _textExtractorClient = textExtractorClient;
            _telemetryClient = telemetryClient;
        }

        [FunctionName(nameof(CompleteIndex))]
        public async Task<bool> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var (payload, targetCount) = context.GetInput<(CaseDocumentOrchestrationPayload, int)>();
            var results = await _textExtractorClient.GetDocumentIndexCount(
                payload.CmsCaseUrn,
                payload.CmsCaseId,
                payload.CmsDocumentId,
                payload.CmsVersionId,
                payload.CorrelationId);

            var isComplete = results.LineCount >= targetCount;

            _telemetryClient.TrackEvent(new VNextDummyEvent(payload.CorrelationId, payload.SubCorrelationId, "CompleteIndex"));

            return isComplete;
        }
    }
}
