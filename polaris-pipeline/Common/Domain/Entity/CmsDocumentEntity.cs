using Newtonsoft.Json;
using Common.Dto.Document;
using Common.Dto.FeatureFlags;
using System.ComponentModel.DataAnnotations;
using Common.ValueObjects;
using Common.Dto.Tracker;

namespace Common.Domain.Entity
{
    public class CmsDocumentEntity : BaseDocumentEntity
    {
        public CmsDocumentEntity()
            : base()
        { }

        public CmsDocumentEntity(
            PolarisDocumentId polarisDocumentId,
            int polarisDocumentVersionId,
            string cmsDocumentId,
            long cmsVersionId,
            DocumentTypeDto cmsDocType,
            string cmsFileCreatedDate,
            string cmsOriginalFileName,
            bool isOcrProcessed,
            PresentationFlagsDto presentationFlags)
            : base(polarisDocumentId, polarisDocumentVersionId, cmsDocumentId, cmsVersionId, presentationFlags)
        {
            CmsDocType = cmsDocType;
            CmsFileCreatedDate = cmsFileCreatedDate;
            CmsOriginalFileName = cmsOriginalFileName;
            IsOcrProcessed = isOcrProcessed;
            Status = TrackerDocumentStatus.New;
        }

        [JsonProperty("cmsDocType")]
        public DocumentTypeDto CmsDocType { get; set; }

        [JsonProperty("cmsOriginalFileName")]
        [Required]
        [RegularExpression(@"^.+\.[A-Za-z]{3,4}$")]
        public string CmsOriginalFileName { get; set; }

        [JsonProperty("cmsFileCreatedDate")]
        public string CmsFileCreatedDate { get; set; }

        [JsonProperty("isOcrProcessed")]
        public bool IsOcrProcessed { get; set; }
    }
}