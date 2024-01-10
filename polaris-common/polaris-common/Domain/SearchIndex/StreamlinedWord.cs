using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace polaris_common.Domain.SearchIndex;

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