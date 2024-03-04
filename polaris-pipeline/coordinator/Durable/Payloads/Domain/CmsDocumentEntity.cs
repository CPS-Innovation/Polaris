using Newtonsoft.Json;
using Common.Dto.Document;
using Common.Dto.FeatureFlags;
using System.ComponentModel.DataAnnotations;
using Common.ValueObjects;

namespace coordinator.Durable.Payloads.Domain
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
            string path,
            string fileExtension,
            string cmsFileCreatedDate,
            string cmsOriginalFileName,
            string presentationTitle,
            bool isOcrProcessed,
            bool isDispatched,
            int? categoryListOrder,
            PolarisDocumentId polarisParentDocumentId,
            string cmsParentDocumentId,
            int? witnessId,
            PresentationFlagsDto presentationFlags,
            bool hasFailedAttachments)
            : base(polarisDocumentId, polarisDocumentVersionId, cmsDocumentId, cmsVersionId, presentationFlags)
        {
            CmsDocType = cmsDocType;
            Path = path;
            CmsOriginalFileExtension = fileExtension;
            CmsFileCreatedDate = cmsFileCreatedDate;
            CmsOriginalFileName = cmsOriginalFileName;
            PresentationTitle = presentationTitle;
            IsOcrProcessed = isOcrProcessed;
            IsDispatched = isDispatched;
            CategoryListOrder = categoryListOrder;
            PolarisParentDocumentId = polarisParentDocumentId;
            CmsParentDocumentId = cmsParentDocumentId;
            WitnessId = witnessId;
            HasFailedAttachments = hasFailedAttachments;
        }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("cmsOriginalFileExtension")]
        public string CmsOriginalFileExtension { get; set; }

        [JsonProperty("cmsDocType")]
        public DocumentTypeDto CmsDocType { get; set; }

        [JsonProperty("cmsOriginalFileName")]
        [Required]
        [RegularExpression(@"^.+\.[A-Za-z]{3,4}$")]
        public string CmsOriginalFileName { get; set; }

        [JsonProperty("presentationTitle")]
        public string PresentationTitle { get; set; }

        [JsonProperty("cmsFileCreatedDate")]
        public string CmsFileCreatedDate { get; set; }

        [JsonProperty("isOcrProcessed")]
        public bool IsOcrProcessed { get; set; }

        [JsonProperty("isDispatched")]
        public bool IsDispatched { get; set; }

        [JsonProperty("categoryListOrder")]
        public int? CategoryListOrder { get; set; }

        [JsonIgnore]
        public PolarisDocumentId PolarisParentDocumentId { get; set; }

        [JsonProperty("polarisParentDocumentId")]
        public string PolarisParentDocumentIdValue
        {
            get
            {
                return PolarisParentDocumentId?.ToString();
            }
            set
            {
                PolarisParentDocumentId = new PolarisDocumentId(value);
            }
        }

        [JsonProperty("cmsParentDocumentId")]
        public string CmsParentDocumentId { get; set; }

        [JsonProperty("witnessId")]
        public int? WitnessId { get; set; }
        [JsonProperty("hasFailedAttachments")]
        public bool HasFailedAttachments { get; set; }
    }
}