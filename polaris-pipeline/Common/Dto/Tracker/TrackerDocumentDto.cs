using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using Common.Dto.Document;
using Common.Dto.FeatureFlags;

namespace Common.Dto.Tracker
{
    public class TrackerDocumentDto : BaseTrackerDocumentDto
    {
        public TrackerDocumentDto(
            Guid polarisDocumentId,
            int polarisDocumentVersionId,
            string cmsDocumentId,
            long cmsVersionId,
            DocumentTypeDto cmsDocType,
            string cmsFileCreatedDate,
            string cmsOriginalFileName,
            PresentationFlagsDto presentationFlags)
            : base(polarisDocumentId, polarisDocumentVersionId, cmsDocumentId, cmsVersionId, presentationFlags)
        {
            CmsDocType = cmsDocType;
            CmsFileCreatedDate = cmsFileCreatedDate;
            CmsOriginalFileName = cmsOriginalFileName;
            Status = TrackerDocumentStatus.New;
        }

        [JsonProperty("cmsDocType")]
        public DocumentTypeDto CmsDocType { get; set; }

        [JsonProperty("cmsOriginalFileName")]
        public string CmsOriginalFileName { get; set; }

        [JsonProperty("cmsFileCreatedDate")]
        public string CmsFileCreatedDate { get; set; }
    }
}