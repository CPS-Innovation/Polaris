using System.Collections.Generic;
using System.Text.Json.Serialization;
using Common.Services.PiiService.Domain.Chunking;

namespace Common.Dto.Request
{
    public class PiiRequestDto
    {
        public PiiRequestDto(string[] piiCategories, List<PiiChunk> piiChunks)
        {
            Parameters = new PiiCategoryParameters(piiCategories);
            AnalysisInput = CreateDocuments(piiChunks);
        }

        [JsonPropertyName("kind")]
        public static string Kind => "PiiEntityRecognition";

        [JsonPropertyName("parameters")]
        public PiiCategoryParameters Parameters { get; set; }

        [JsonPropertyName("analysisInput")]
        public AnalysisInput AnalysisInput { get; set; }

        private static AnalysisInput CreateDocuments(List<PiiChunk> piiChunks)
        {
            var analysisInput = new AnalysisInput();
            var documentId = 1;

            foreach (var piiChunk in piiChunks)
            {
                analysisInput.Documents.Add(new AnalysisDocument
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

        [JsonPropertyName("piiCategories")]
        public string[] PiiCategories { get; set; }

        [JsonPropertyName("modelVersion")]
        public static string ModelVersion => "latest";
    }

    public class AnalysisInput
    {
        [JsonPropertyName("documents")]
        public List<AnalysisDocument> Documents { get; set; } = new List<AnalysisDocument>();
    }

    public class AnalysisDocument
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("language")]
        public static string Language => "en";

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}