using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.AI.TextAnalytics;
using Common.Dto.Request;

namespace Common.Services.PiiService.TextAnalytics
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
            List<TextDocumentInput> batchedDocuments = [];

            foreach (var document in piiRequest.AnalysisInput.Documents)
            {
                batchedDocuments.Add(new TextDocumentInput(document.Id, document.Text)
                {
                    Language = document.Language
                });
            }

            RecognizePiiEntitiesOptions options = new() { IncludeStatistics = true };

            foreach (var category in piiRequest.Parameters.PiiCategories)
            {
                options.CategoriesFilter.Add(category);
            }

            return await _textAnalyticsClient.RecognizePiiEntitiesBatchAsync(batchedDocuments, options);
        }
    }
}