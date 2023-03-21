using Common.Domain.DocumentExtraction;
using coordinator.Domain.Tracker.Presentation;
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
            PresentationFlags = new PresentationFlags();
        }

        public TrackerDocument(
            Guid polarisDocumentId,
            string cmsDocumentId,
            long cmsVersionId,
            CmsDocType cmsDocType,
            string cmsMimeType,
            string cmsFileExtension,
            string cmsFileCreatedDate,
            string cmsOriginalFileName,
            PresentationFlags presentationFlags)
        {
            PolarisDocumentId = polarisDocumentId;
            CmsDocumentId = cmsDocumentId;
            CmsVersionId = cmsVersionId;
            CmsDocType = cmsDocType;
            CmsMimeType = cmsMimeType;
            CmsFileExtension = cmsFileExtension;
            CmsFileCreatedDate = cmsFileCreatedDate;
            CmsOriginalFileName = cmsOriginalFileName;
            Status = DocumentStatus.None;
            PresentationFlags = presentationFlags;
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
        public string CmsFileExtension { get; set; }

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
        public PresentationFlags PresentationFlags { get; set; }
    }
}