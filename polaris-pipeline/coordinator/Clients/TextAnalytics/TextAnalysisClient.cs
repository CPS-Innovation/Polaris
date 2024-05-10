using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.AI.TextAnalytics;
using coordinator.Domain;
using coordinator.Factories.TextAnalyticsClientFactory;

namespace coordinator.Clients.TextAnalytics
{
    public class TextAnalysisClient : ITextAnalysisClient
    {
        private readonly TextAnalyticsClient _textAnalyticsClient;

        public TextAnalysisClient(ITextAnalyticsClientFactory textAnalyticsClientFactory)
        {
            _textAnalyticsClient = textAnalyticsClientFactory.Create();
        }

        public async Task<RecognizePiiEntitiesResultCollection> CheckForPii(PiiRequestDto piiRequest)
        {
            List<TextDocumentInput> batchedDocuments = new();

            foreach (var document in piiRequest.AnalysisInput.Documents)
            {
                batchedDocuments.Add(new TextDocumentInput(document.Id, document.Text)
                {
                    Language = document.Language
                });
            }

            RecognizePiiEntitiesOptions options = new() { IncludeStatistics = true };

            return await _textAnalyticsClient.RecognizePiiEntitiesBatchAsync(batchedDocuments, options);
        }
    }
}