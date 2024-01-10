using Newtonsoft.Json;

namespace polaris_common.Dto.Request.Search
{
    public class SearchRequestDocumentDto
    {
        [JsonProperty("cmsDocumentId")]
        public string CmsDocumentId { get; set; }

        [JsonProperty("cmsVersionId")]
        public long CmsVersionId { get; set; }
    }
}