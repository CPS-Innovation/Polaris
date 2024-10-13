using System.ComponentModel.DataAnnotations;
using Common.Dto.Document;
using Common.Dto.FeatureFlags;
using Common.ValueObjects;
using Newtonsoft.Json;

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
            bool hasFailedAttachments,
            bool hasNotes,
            bool isUnused,
            bool isInbox,
            string classification,
            bool isWitnessManagement,
            bool canReclassify,
            bool canRename,
            string renameStatus,
            string reference)
            : base(polarisDocumentId, polarisDocumentVersionId, cmsDocumentId, cmsVersionId, presentationFlags)
        {
            CmsDocType = cmsDocType;
            Path = path;
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
            HasNotes = hasNotes;
            IsUnused = isUnused;
            IsInbox = isInbox;
            Classification = classification;
            IsWitnessManagement = isWitnessManagement;
            CanReclassify = canReclassify;
            CanRename = canRename;
            RenameStatus = renameStatus;
            Reference = reference;
        }

        [JsonProperty("path")]
        public string Path { get; set; }

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

        [JsonProperty("hasNotes")]
        public bool HasNotes { get; set; }

        [JsonProperty("isUnused")]
        public bool IsUnused { get; set; }

        [JsonProperty("isInbox")]
        public bool IsInbox { get; set; }

        [JsonProperty("classification")]
        public string Classification { get; set; }

        [JsonProperty("isWitnessManagement")]
        public bool IsWitnessManagement { get; set; }

        [JsonProperty("canReclassify")]
        public bool CanReclassify { get; set; }

        [JsonProperty("canRename")]
        public bool CanRename { get; set; }

        [JsonProperty("renameStatus")]
        public string RenameStatus { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }
    }
}