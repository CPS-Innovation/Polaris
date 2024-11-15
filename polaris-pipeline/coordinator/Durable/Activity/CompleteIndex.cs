using System.Threading.Tasks;
using coordinator.Clients.TextExtractor;
using coordinator.Durable.Payloads;
using Microsoft.Azure.Functions.Worker;

namespace coordinator.Durable.Activity
{
    public class CompleteIndex
    {
        private readonly ITextExtractorClient _textExtractorClient;

        public CompleteIndex(ITextExtractorClient textExtractorClient)
        {
            _textExtractorClient = textExtractorClient;
        }

        [Function(nameof(CompleteIndex))]
        public async Task<(bool, long)> Run([ActivityTrigger] DocumentPayload payload, int targetCount)
        {
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
