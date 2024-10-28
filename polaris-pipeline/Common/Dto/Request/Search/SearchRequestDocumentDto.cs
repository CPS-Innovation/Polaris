using Newtonsoft.Json;

namespace Common.Dto.Request.Search
{
    public class SearchRequestDocumentDto
    {
        [JsonProperty("documentId")]
        public string DocumentId { get; set; }

        [JsonProperty("versionId")]
        public long VersionId { get; set; }
    }
}