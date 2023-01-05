using System.Collections.Generic;
using Newtonsoft.Json;

namespace RumpoleGateway.Domain.RumpolePipeline
{
    public class StreamlinedSearchLine
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("caseId")]
        public long CaseId { get; set; }

        [JsonProperty("documentId")]
        public string DocumentId { get; set; }
    
        [JsonProperty("versionId")]
        public long VersionId { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }
    
        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; }

        [JsonProperty("lineIndex")]
        public int LineIndex { get; set; }

        [JsonProperty("pageHeight")]
        public double PageHeight { get; set; }

        [JsonProperty("pageWidth")]
        public double PageWidth { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "words")]
        public IList<StreamlinedWord> Words { get; set; }
    }
}
