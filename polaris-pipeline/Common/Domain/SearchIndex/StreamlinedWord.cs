using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Common.Domain.SearchIndex;

public class StreamlinedWord
{
    [JsonPropertyName("boundingBox")]
    public IList<double?> BoundingBox { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("matchType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public StreamlinedMatchType StreamlinedMatchType { get; set; }
}