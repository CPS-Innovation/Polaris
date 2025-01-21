using System.Text.Json.Serialization;

namespace Common.Dto.Request.Redaction
{
    public class RedactionCoordinatesDto
    {
        [JsonPropertyName("x1")]
        public double X1 { get; set; }

        [JsonPropertyName("y1")]
        public double Y1 { get; set; }

        [JsonPropertyName("x2")]
        public double X2 { get; set; }

        [JsonPropertyName("y2")]
        public double Y2 { get; set; }
    }
}
