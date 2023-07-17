using Newtonsoft.Json;

namespace Common.Dto.Request.Search
{
    public class SearchRequestDocumentDto
    {
        [JsonProperty("cmsDocumentId")]
        public string CmsDocumentId { get; set; }

        [JsonProperty("cmsVersionId")]
        public long CmsVersionId { get; set; }
    }
}