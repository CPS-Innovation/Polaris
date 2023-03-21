using System;
using Common.Domain.Pipeline;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PolarisGateway.Domain.PolarisPipeline
{
    public class TrackerDocument
    {
        [JsonProperty("polarisDocumentId")]
        public Guid PolarisDocumentId { get; set; }

        [JsonProperty("cmsDocumentId")]
        public string CmsDocumentId { get; set; }

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

        [JsonProperty("isPdfReady")]
        public bool IsPdfAvailable { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public DocumentStatus Status { get; set; }
    }
}