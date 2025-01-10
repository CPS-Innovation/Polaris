using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Common.Dto.Request.Search
{
    public class SearchRequestDocumentDto
    {
        [JsonProperty("documentId")]
        [JsonPropertyName("documentId")]
        public string DocumentId { get; set; }

        [JsonProperty("versionId")]
        [JsonPropertyName("versionId")]
        public long VersionId { get; set; }
    }
}