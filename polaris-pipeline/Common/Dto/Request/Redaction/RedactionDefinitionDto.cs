using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Common.Dto.Request.Redaction
{
    public class RedactionDefinitionDto
    {
        [JsonProperty("pageIndex")]
        [JsonPropertyName("pageIndex")]
        public int PageIndex { get; set; }

        [JsonProperty("width")]
        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonProperty("height")]
        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonProperty("redactionCoordinates")]
        [JsonPropertyName("redactionCoordinates")]
        public List<RedactionCoordinatesDto> RedactionCoordinates { get; set; }
    }
}
