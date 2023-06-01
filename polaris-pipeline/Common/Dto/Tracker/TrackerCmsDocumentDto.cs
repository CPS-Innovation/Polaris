using Newtonsoft.Json;
using System;
using Common.Dto.Document;
using Common.Dto.FeatureFlags;
using System.ComponentModel.DataAnnotations;
using Common.ValueObjects;

namespace Common.Dto.Tracker
{
    public class TrackerCmsDocumentDto : BaseTrackerDocumentDto
    {
        public TrackerCmsDocumentDto()
            : base()
        { }

        public TrackerCmsDocumentDto(
            PolarisDocumentId polarisDocumentId,
            int polarisDocumentVersionId,
            string cmsDocumentId,
            long cmsVersionId,
            DocumentTypeDto cmsDocType,
            string cmsFileCreatedDate,
            string title,
            bool isOcrProcessed,
            PresentationFlagsDto presentationFlags)
            : base(polarisDocumentId, polarisDocumentVersionId, cmsDocumentId, cmsVersionId, presentationFlags)
        {
            CmsDocType = cmsDocType;
            CmsFileCreatedDate = cmsFileCreatedDate;
            Title = title;
            IsOcrProcessed = isOcrProcessed;
            Status = TrackerDocumentStatus.New;
        }

        [JsonProperty("cmsDocType")]
        public DocumentTypeDto CmsDocType { get; set; }

        [JsonProperty("presentationFileName")]
        [Required]
        [RegularExpression(@"^.+\.[A-Za-z]{3,4}$")]
        public string Title { get; set; }

        [JsonProperty("cmsFileCreatedDate")]
        public string CmsFileCreatedDate { get; set; }

        [JsonProperty("isOcrProcessed")]
        public bool IsOcrProcessed { get; set; }
    }
}