using Common.Dto.Response.Document.FeatureFlags;

namespace Common.Dto.Response.Document
{
    public class CmsDocumentDto
    {
        public CmsDocumentDto()
        {
        }

        public long DocumentId { get; set; }

        public long VersionId { get; set; }

        public string Path { get; set; } /* DO NOT LEAK */

        public string FileName { get; set; } /* CmsOriginalFileName */

        public string PresentationTitle { get; set; }

        public string MimeType { get; set; } /* not reqd */

        public string FileExtension { get; set; } /* not reqd */

        public DocumentTypeDto CmsDocType { get; set; }

        public string DocumentDate { get; set; } /* CmsFileCreatedDate */

        public bool IsOcrProcessed { get; set; }

        public bool IsDispatched { get; set; }

        public int? CategoryListOrder { get; set; }

        public string ParentDocumentId { get; set; }

        public int? WitnessId { get; set; }

        public PresentationFlagsDto PresentationFlags { get; set; }

        public bool HasFailedAttachments { get; set; }

        public bool HasNotes { get; set; }

        public bool IsUnused { get; set; }

        public bool IsInbox { get; set; }

        public string Classification { get; set; }

        public bool IsWitnessManagement { get; set; }

        public bool CanReclassify { get; set; }

        public bool CanRename { get; set; }

        public string RenameStatus { get; set; }

        public string Reference { get; set; }

        public string Title { get; set; }
    }
}