using System.Collections.Generic;
using Newtonsoft.Json;

namespace Common.Dto.Request.Redaction
{
    public class RedactionDefinitionDto
    {
        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; }

        [JsonProperty("width")]
        public double Width { get; set; }

        [JsonProperty("height")]
        public double Height { get; set; }

        [JsonProperty("redactionCoordinates")]
        public List<RedactionCoordinatesDto> RedactionCoordinates { get; set; }
    }
}
