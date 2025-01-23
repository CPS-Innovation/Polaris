using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Common.Domain.Pii
{
    public class PiiLine
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("fileName")]
        public string FileName { get; set; }

        [JsonPropertyName("pageIndex")]
        public int PageIndex { get; set; }

        [JsonPropertyName("lineIndex")]
        public int LineIndex { get; set; }

        [JsonIgnore]
        public int AccumulativeLineIndex { get; set; }

        [JsonPropertyName("pageHeight")]
        public double PageHeight { get; set; }

        [JsonPropertyName("pageWidth")]
        public double PageWidth { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("words")]
        public IList<PiiWord> Words { get; set; }
    }
}