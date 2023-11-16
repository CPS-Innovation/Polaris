using Common.Dto.FeatureFlags;

namespace Common.Dto.Document
{
    public class CmsDocumentDto
    {
        public string DocumentId { get; set; }

        public long VersionId { get; set; }

        public string Path { get; set; }

        public string FileName { get; set; }

        public string PresentationTitle { get; set; }

        public string MimeType { get; set; }

        public string FileExtension { get; set; }

        public DocumentTypeDto CmsDocType { get; set; }

        public string DocumentDate { get; set; }

        public bool IsOcrProcessed { get; set; }

        public bool IsDispatched { get; set; }

        public int? CategoryListOrder { get; set; }

        public string ParentDocumentId { get; set; }

        public PresentationFlagsDto PresentationFlags { get; set; }
    }
}