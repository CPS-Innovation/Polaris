using System.Text.Json.Serialization;

namespace Common.Dto.Request.Search
{
    public class SearchRequestDocumentDto
    {
        [JsonPropertyName("documentId")]
        public string DocumentId { get; set; }

        [JsonPropertyName("versionId")]
        public long VersionId { get; set; }
    }
}