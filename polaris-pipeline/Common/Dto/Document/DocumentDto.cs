using Common.Dto.FeatureFlags;

namespace Common.Dto.Document
{
    public class DocumentDto
    {
        public string DocumentId { get; set; }

        public long VersionId { get; set; }

        public string FileName { get; set; }

        public string MimeType { get; set; }

        public string FileExtension { get; set; }

        public DocumentTypeDto CmsDocType { get; set; }

        public string DocumentDate { get; set; }

        public PresentationFlagsDto PresentationFlags { get; set; }
    }
}