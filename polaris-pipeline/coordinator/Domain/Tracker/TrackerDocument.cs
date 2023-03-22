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
            PresentationFlags = presentationFlags;
            Status = DocumentStatus.New;
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

        [JsonProperty("cmsFileExtension")]
        public string CmsFileExtension { get; set; }

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

        [JsonProperty("presentationFlags")]
        public PresentationFlags PresentationFlags { get; set; }
    }
}