using System.Collections.Generic;
using Newtonsoft.Json;

namespace Common.Domain.Redaction
{
    public class RedactionDefinition
    {
        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; }

        [JsonProperty("width")]
        public double Width { get; set; }

        [JsonProperty("height")]
        public double Height { get; set; }

        [JsonProperty("redactionCoordinates")]
        public List<RedactionCoordinates> RedactionCoordinates { get; set; }
    }
}
