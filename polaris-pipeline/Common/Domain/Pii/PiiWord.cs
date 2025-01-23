using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Common.Domain.SearchIndex;

namespace Common.Domain.Pii
{
    public class PiiWord
    {
        [JsonPropertyName("boundingBox")]
        public IList<double?> BoundingBox { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
        [JsonPropertyName("sanitizedText")]
        public string SanitizedText { get; set; }

        [JsonPropertyName("matchType")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public StreamlinedMatchType StreamlinedMatchType { get; set; }

        [JsonPropertyName( "piiCategory")]
        public string PiiCategory { get; set; }

        [JsonPropertyName("redactionType")]
        public string RedactionType { get; set; }

        [JsonPropertyName("piiGroupId")]
        public Guid? PiiGroupId { get; set; }
    }
}