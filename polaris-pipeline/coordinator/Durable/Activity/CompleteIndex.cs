using System.Threading.Tasks;
using coordinator.Clients.TextExtractor;
using coordinator.Durable.Payloads;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Durable.Activity
{
    public class CompleteIndex
    {
        private readonly ITextExtractorClient _textExtractorClient;

        public CompleteIndex(ITextExtractorClient textExtractorClient)
        {
            _textExtractorClient = textExtractorClient;
        }

        [FunctionName(nameof(CompleteIndex))]
        public async Task<(bool, long)> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var (payload, targetCount) = context.GetInput<(CaseDocumentOrchestrationPayload, int)>();
            var results = await _textExtractorClient.GetDocumentIndexCount(
                payload.Urn,
                payload.CaseId,
                payload.DocumentId,
                payload.VersionId,
                payload.CorrelationId);

            var isComplete = results.LineCount >= targetCount;

            return (isComplete, results.LineCount);
        }
    }
}
