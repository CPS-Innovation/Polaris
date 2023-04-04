using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using Common.Dto.Document;
using Common.Dto.FeatureFlags;

namespace Common.Dto.Tracker
{
    public class TrackerDocumentDto
    {
        public TrackerDocumentDto()
        {
            CmsDocType = new DocumentTypeDto();
            PresentationFlags = new PresentationFlagsDto();
        }

        public TrackerDocumentDto(
            Guid polarisDocumentId,
            int polarisDocumentVersionId,
            string cmsDocumentId,
            long cmsVersionId,
            DocumentTypeDto cmsDocType,
            string cmsMimeType,
            string cmsFileExtension,
            string cmsFileCreatedDate,
            string cmsOriginalFileName,
            PresentationFlagsDto presentationFlags)
        {
            PolarisDocumentId = polarisDocumentId;
            PolarisDocumentVersionId = polarisDocumentVersionId;
            CmsDocumentId = cmsDocumentId;
            CmsVersionId = cmsVersionId;
            CmsDocType = cmsDocType;
            CmsMimeType = cmsMimeType;
            CmsFileExtension = cmsFileExtension;
            CmsFileCreatedDate = cmsFileCreatedDate;
            CmsOriginalFileName = cmsOriginalFileName;
            PresentationFlags = presentationFlags;
            Status = TrackerDocumentStatus.New;
        }

        [JsonProperty("polarisDocumentId")]
        public Guid PolarisDocumentId { get; set; }

        [JsonProperty("polarisDocumentVersionId")]
        public int PolarisDocumentVersionId { get; set; }

        [JsonProperty("cmsDocumentId")]
        public string CmsDocumentId { get; set; }

        [JsonProperty("cmsVersionId")]
        public long CmsVersionId { get; set; }

        [JsonProperty("cmsDocType")]
        public DocumentTypeDto CmsDocType { get; set; }

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
        public TrackerDocumentStatus Status { get; set; }

        [JsonProperty("presentationFlags")]
        public PresentationFlagsDto PresentationFlags { get; set; }
    }
}