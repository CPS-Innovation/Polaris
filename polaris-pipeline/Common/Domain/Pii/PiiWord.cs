using System;
using System.Collections.Generic;
using Common.Domain.SearchIndex;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Common.Domain.Pii
{
    public class PiiWord
    {
        [JsonProperty(PropertyName = "boundingBox")]
        public IList<double?> BoundingBox { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "matchType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public StreamlinedMatchType StreamlinedMatchType { get; set; }

        [JsonProperty(PropertyName = "piiCategory")]
        public string PiiCategory { get; set; }

        [JsonProperty(PropertyName = "redactionType")]
        public string RedactionType { get; set; }

        [JsonProperty(PropertyName = "piiGroupId")]
        public Guid? PiiGroupId { get; set; }
    }
}