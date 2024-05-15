using System.Collections.Generic;
using coordinator.Services.OcrResultsService;
using Newtonsoft.Json;

namespace coordinator.Domain
{
    public class PiiRequestDto
    {
        public PiiRequestDto(string[] piiCategories, List<PiiChunk> piiChunks)
        {
            Parameters = new PiiCategoryParameters(piiCategories);
            AnalysisInput = CreateDocuments(piiChunks);
        }

        [JsonProperty("kind")]
        public static string Kind => "PiiEntityRecognition";

        [JsonProperty("parameters")]
        public PiiCategoryParameters Parameters { get; set; }

        [JsonProperty("analysisInput")]
        public AnalysisInput AnalysisInput { get; set; }

        private AnalysisInput CreateDocuments(List<PiiChunk> piiChunks)
        {
            var analysisInput = new AnalysisInput();
            var documentId = 1;

            foreach (var piiChunk in piiChunks)
            {
                analysisInput.Documents.Add(new Document
                {
                    Id = documentId++.ToString(),
                    Text = piiChunk.Text
                });
            }

            return analysisInput;
        }
    }

    public class PiiCategoryParameters
    {
        public PiiCategoryParameters(string[] piiCategories)
        {
            PiiCategories = piiCategories;
        }

        [JsonProperty("piiCategories")]
        public string[] PiiCategories { get; set; }

        [JsonProperty("modelVersion")]
        public static string ModelVersion => "latest";
    }

    public class AnalysisInput
    {
        [JsonProperty("documents")]
        public List<Document> Documents { get; set; } = new List<Document>();
    }

    public class Document
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("language")]
        public string Language => "en";

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}