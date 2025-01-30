using System.Text.Json.Serialization;
using Common.Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Common.Dto.Response.Documents
{
    public class TrackerDocumentDto
    {
        public TrackerDocumentDto()
        {
        }

        [JsonProperty("documentId")]
        [JsonPropertyName("documentId")]
        public string DocumentId { get; set; }

        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonProperty("status")]
        [JsonPropertyName("status")]
        public DocumentStatus Status { get; set; }

        [JsonProperty("versionId")]
        [JsonPropertyName("versionId")]
        public long VersionId { get; set; }

        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonProperty("conversionStatus")]
        public PdfConversionStatus ConversionStatus { get; set; }
    }
}