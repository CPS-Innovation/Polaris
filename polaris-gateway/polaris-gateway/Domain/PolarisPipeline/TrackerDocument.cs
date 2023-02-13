using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PolarisGateway.Domain.PolarisPipeline
{
    public class TrackerDocument
    {
        [JsonProperty("cmsDocumentId")]
        public int CmsDocumentId { get; set; }

        [JsonProperty("cmsVersionId")]
        public long CmsVersionId { get; set; }

        [JsonProperty("cmsDocType")]
        public CmsDocType CmsDocType { get; set; }

        [JsonProperty("cmsMimeType")]
        public string CmsMimeType { get; set; }

        [JsonProperty("cmsOriginalFileName")]
        public string CmsOriginalFileName { get; set; }

        [JsonProperty("cmsFileCreatedDate")]
        public string CmsFileCreatedDate { get; set; }

        [JsonProperty("pdfBlobName")]
        public string PdfBlobName { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public DocumentStatus Status { get; set; }
    }
}

