using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Newtonsoft.Json;

namespace text_extractor.Domain
{
    public class SearchDocument : AnalyzeResults
    {
        [JsonProperty("caseId")]
        public long caseId { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}