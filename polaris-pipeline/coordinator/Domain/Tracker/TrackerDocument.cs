using Common.Domain.DocumentExtraction;
using coordinator.Domain.Tracker.PresentationStatus;
using Common.Domain.Pipeline;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace coordinator.Domain.Tracker
{
    public class TrackerDocument
    {
        // todo: this is just here for testing
        public TrackerDocument()
        {
            CmsDocType = new CmsDocType();
            PresentationStatuses = new PresentationStatuses();
        }

        public TrackerDocument(Guid polarisDocumentId, string cmsDocumentId, long cmsVersionId, CmsDocType cmsDocType, string cmsMimeType, string cmsFileCreatedDate, string cmsOriginalFileName)
        {
            PolarisDocumentId = polarisDocumentId;
            CmsDocumentId = cmsDocumentId;
            CmsVersionId = cmsVersionId;
            CmsDocType = cmsDocType;
            CmsMimeType = cmsMimeType;
            CmsFileCreatedDate = cmsFileCreatedDate;
            CmsOriginalFileName = cmsOriginalFileName;
            Status = DocumentStatus.None;
            PresentationStatuses = new PresentationStatuses();
        }

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

        [JsonProperty("cmsDocumentExtension")]
        public string CmsDocumentExtension { get; set; }

        [JsonProperty("cmsOriginalFileName")]
        public string CmsOriginalFileName { get; set; }

        [JsonProperty("cmsFileCreatedDate")]
        public string CmsFileCreatedDate { get; set; }

        [JsonProperty("pdfBlobName")]
        public string PdfBlobName { get; set; }

        [JsonProperty("isPdfAvailable")]
        public bool IsPdfAvailable { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public DocumentStatus Status { get; set; }

        [JsonProperty("presentationStatuses")]
        public PresentationStatuses PresentationStatuses { get; set; }
    }
}