using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.AI.TextAnalytics;
using coordinator.Domain;

namespace coordinator.Clients.TextAnalytics
{
    public class TextAnalysisClient : ITextAnalysisClient
    {
        private readonly string Key = "4a3c4e0627934d36b0da46a5074fcfd8";
        private readonly string EndpointUri = "https://lang-polaris-pipeline-dev.cognitiveservices.azure.com/";
        private readonly TextAnalyticsClient _textAnalyticsClient;

        public TextAnalysisClient()
        {
            _textAnalyticsClient = new TextAnalyticsClient(new System.Uri(EndpointUri), new AzureKeyCredential(Key));
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