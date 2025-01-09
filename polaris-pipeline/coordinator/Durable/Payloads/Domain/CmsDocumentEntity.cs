using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Common.Domain.Document;
using Common.Dto.Response.Document;
using Common.Dto.Response.Document.FeatureFlags;

namespace coordinator.Durable.Payloads.Domain
{
    public class CmsDocumentEntity : BaseDocumentEntity
    {
        public CmsDocumentEntity() : base()
        {
        }

        public CmsDocumentEntity(
           long cmsDocumentId,
           long versionId,
           PresentationFlagsDto presentationFlags)
       : base(cmsDocumentId, versionId, presentationFlags) { }

        public CmsDocumentEntity(
            long cmsDocumentId,
            long versionId,
            DocumentTypeDto cmsDocType,
            string path,
            string cmsFileCreatedDate,
            string cmsOriginalFileName,
            string presentationTitle,
            bool isOcrProcessed,
            bool isDispatched,
            int? categoryListOrder,
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
            : base(cmsDocumentId, versionId, presentationFlags)
        {
            CmsDocType = cmsDocType;
            Path = path;
            CmsFileCreatedDate = cmsFileCreatedDate;
            CmsOriginalFileName = cmsOriginalFileName;
            PresentationTitle = presentationTitle;
            IsOcrProcessed = isOcrProcessed;
            IsDispatched = isDispatched;
            CategoryListOrder = categoryListOrder;
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

        [JsonPropertyName("documentId")]
        public override string DocumentId => DocumentNature.ToQualifiedStringDocumentId(CmsDocumentId, DocumentNature.Types.Document);

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("cmsDocType")]
        public DocumentTypeDto CmsDocType { get; set; }

        [JsonPropertyName("cmsOriginalFileName")]
        [Required]
        [RegularExpression(@"^.+\.[A-Za-z]{3,4}$")]
        public string CmsOriginalFileName { get; set; }

        [JsonPropertyName("cmsFileCreatedDate")]
        public string CmsFileCreatedDate { get; set; }

        [JsonPropertyName("isOcrProcessed")]
        public bool IsOcrProcessed { get; set; }

        [JsonPropertyName("isDispatched")]
        public bool IsDispatched { get; set; }

        [JsonPropertyName("categoryListOrder")]
        public int? CategoryListOrder { get; set; }

        [JsonPropertyName("parentDocumentId")]
        public string ParentDocumentId => string.IsNullOrWhiteSpace(CmsParentDocumentId) ?
            null :
            DocumentNature.ToQualifiedStringDocumentId(CmsParentDocumentId, DocumentNature.Types.Document);

        [JsonPropertyName("cmsParentDocumentId")]
        public string CmsParentDocumentId { get; set; }

        [JsonPropertyName("presentationTitle")]
        public string PresentationTitle { get; set; }

        [JsonPropertyName("witnessId")]
        public int? WitnessId { get; set; }

        [JsonPropertyName("hasFailedAttachments")]
        public bool HasFailedAttachments { get; set; }

        [JsonPropertyName("hasNotes")]
        public bool HasNotes { get; set; }

        [JsonPropertyName("isUnused")]
        public bool IsUnused { get; set; }

        [JsonPropertyName("isInbox")]
        public bool IsInbox { get; set; }

        [JsonPropertyName("classification")]
        public string Classification { get; set; }

        [JsonPropertyName("isWitnessManagement")]
        public bool IsWitnessManagement { get; set; }

        [JsonPropertyName("canReclassify")]
        public bool CanReclassify { get; set; }

        [JsonPropertyName("canRename")]
        public bool CanRename { get; set; }

        [JsonPropertyName("renameStatus")]
        public string RenameStatus { get; set; }

        [JsonPropertyName("reference")]
        public string Reference { get; set; }
    }
}