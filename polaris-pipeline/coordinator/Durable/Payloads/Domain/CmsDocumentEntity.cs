//using System.ComponentModel.DataAnnotations;
using Common.Domain.Document;
//using Common.Dto.Response.Document;
//using Common.Dto.Response.Document.FeatureFlags;
using Newtonsoft.Json;

namespace coordinator.Durable.Payloads.Domain
{
    public class CmsDocumentEntity : BaseDocumentEntity
    {
        public CmsDocumentEntity()
            : base()
        { }

        public CmsDocumentEntity(
           long cmsDocumentId,
           long versionId
           //PresentationFlagsDto presentationFlags
           )
       : base(cmsDocumentId, versionId/*, presentationFlags*/) { }

        public CmsDocumentEntity(
            long cmsDocumentId,
            long versionId,
            //DocumentTypeDto cmsDocType,
            string path
            // string cmsFileCreatedDate,
            // string cmsOriginalFileName,
            // string presentationTitle,
            // bool isOcrProcessed,
            // bool isDispatched,
            // int? categoryListOrder,
            // string cmsParentDocumentId,
            // int? witnessId,
            // PresentationFlagsDto presentationFlags,
            // bool hasFailedAttachments,
            // bool hasNotes,
            // bool isUnused,
            // bool isInbox,
            // string classification,
            // bool isWitnessManagement,
            // bool canReclassify,
            // bool canRename,
            // string renameStatus,
            // string reference
            )
            : base(cmsDocumentId, versionId/*, presentationFlags*/)
        {
            //CmsDocType = cmsDocType;
            Path = path;
            // CmsFileCreatedDate = cmsFileCreatedDate;
            // CmsOriginalFileName = cmsOriginalFileName;
            // PresentationTitle = presentationTitle;
            // IsOcrProcessed = isOcrProcessed;
            // IsDispatched = isDispatched;
            // CategoryListOrder = categoryListOrder;
            // CmsParentDocumentId = cmsParentDocumentId;
            // WitnessId = witnessId;
            // HasFailedAttachments = hasFailedAttachments;
            // HasNotes = hasNotes;
            // IsUnused = isUnused;
            // IsInbox = isInbox;
            // Classification = classification;
            // IsWitnessManagement = isWitnessManagement;
            // CanReclassify = canReclassify;
            // CanRename = canRename;
            // RenameStatus = renameStatus;
            // Reference = reference;
        }
        public override string DocumentId => DocumentNature.ToQualifiedStringDocumentId(CmsDocumentId, DocumentNature.Types.Document);

        [JsonProperty("path")]
        public string Path { get; set; }

        // [JsonProperty("cmsDocType")]
        // public DocumentTypeDto CmsDocType { get; set; }

        // [JsonProperty("cmsOriginalFileName")]
        // [Required]
        // [RegularExpression(@"^.+\.[A-Za-z]{3,4}$")]
        // public string CmsOriginalFileName { get; set; }

        // [JsonProperty("cmsFileCreatedDate")]
        // public string CmsFileCreatedDate { get; set; }

        // [JsonProperty("isOcrProcessed")]
        // public bool IsOcrProcessed { get; set; }

        // [JsonProperty("isDispatched")]
        // public bool IsDispatched { get; set; }

        // [JsonProperty("categoryListOrder")]
        // public int? CategoryListOrder { get; set; }

        // [JsonProperty("parentDocumentId")]
        // public string ParentDocumentId
        // {
        //     get => string.IsNullOrWhiteSpace(CmsParentDocumentId)
        //         ? null
        //         : DocumentNature.ToQualifiedStringDocumentId(CmsParentDocumentId, DocumentNature.Types.Document);
        // }

        // [JsonProperty("cmsParentDocumentId")]
        // public string CmsParentDocumentId { get; set; }

        // [JsonProperty("presentationTitle")]
        // public string PresentationTitle { get; set; }

        // [JsonProperty("witnessId")]
        // public int? WitnessId { get; set; }

        // [JsonProperty("hasFailedAttachments")]
        // public bool HasFailedAttachments { get; set; }

        // [JsonProperty("hasNotes")]
        // public bool HasNotes { get; set; }

        // [JsonProperty("isUnused")]
        // public bool IsUnused { get; set; }

        // [JsonProperty("isInbox")]
        // public bool IsInbox { get; set; }

        // [JsonProperty("classification")]
        // public string Classification { get; set; }

        // [JsonProperty("isWitnessManagement")]
        // public bool IsWitnessManagement { get; set; }

        // [JsonProperty("canReclassify")]
        // public bool CanReclassify { get; set; }

        // [JsonProperty("canRename")]
        // public bool CanRename { get; set; }

        // [JsonProperty("renameStatus")]
        // public string RenameStatus { get; set; }

        // [JsonProperty("reference")]
        // public string Reference { get; set; }

    }
}