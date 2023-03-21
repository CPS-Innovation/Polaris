namespace Common.Domain.DocumentExtraction
{
    public class CmsCaseDocument
    {
        public string DocumentId { get; set; }

        public long VersionId { get; set; }

        public string FileName { get; set; }

        public string MimeType { get; set; }

        public string FileExtension { get; set; }

        public CmsDocType CmsDocType { get; set; }

        public string DocumentDate { get; set; }
    }
}

