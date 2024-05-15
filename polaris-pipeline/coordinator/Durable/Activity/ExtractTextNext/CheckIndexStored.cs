using System.Threading.Tasks;
using coordinator.Clients.TextExtractor;
using coordinator.Durable.Payloads;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Durable.Activity.ExtractTextNext
{
    public class CheckIndexStored
    {
        private readonly ITextExtractorClient _textExtractorClient;

        public CheckIndexStored(ITextExtractorClient textExtractorClient)
        {
            _textExtractorClient = textExtractorClient;
        }

        [FunctionName(nameof(CheckIndexStored))]
        public async Task<bool> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var (payload, targetCount) = context.GetInput<(CaseDocumentOrchestrationPayload, int)>();
            var results = await _textExtractorClient.GetDocumentIndexCount(
                payload.CmsCaseUrn,
                payload.CmsCaseId,
                payload.CmsDocumentId,
                payload.CmsVersionId,
                payload.CorrelationId);

            return results.LineCount >= targetCount;
        }
    }
}
