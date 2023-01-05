using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PolarisGateway.Domain.PolarisPipeline
{
    public class StreamlinedWord
    {
        [JsonProperty(PropertyName = "boundingBox")]
        public IList<double?> BoundingBox { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "matchType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public StreamlinedMatchType StreamlinedMatchType { get; set; }
    }
}
