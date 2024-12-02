using System.Threading.Tasks;
using coordinator.Clients.TextExtractor;
using coordinator.Domain;
using coordinator.Durable.Orchestration;
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
        public async Task<PollingActivityResult<CompleteIndexResponse>> Run([ActivityTrigger] CompleteIndexPayload payload)
        {
            var results = await _textExtractorClient.GetDocumentIndexCount(
                payload.Payload.Urn,
                payload.Payload.CaseId,
                payload.Payload.DocumentId,
                payload.Payload.VersionId,
                payload.Payload.CorrelationId);

            var isComplete = results.LineCount >= payload.TargetCount;

            return new PollingActivityResult<CompleteIndexResponse> 
            {
                IsCompleted = isComplete,
                Result = new CompleteIndexResponse { IsCompleted = isComplete, LineCount = results.LineCount }
            };
        }
    }
}
