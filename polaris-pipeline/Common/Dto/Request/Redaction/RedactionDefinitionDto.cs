using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Common.Dto.Request.Redaction
{
    public class RedactionDefinitionDto
    {
        [JsonPropertyName("pageIndex")]
        public int PageIndex { get; set; }

        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("redactionCoordinates")]
        public List<RedactionCoordinatesDto> RedactionCoordinates { get; set; }
    }
}
