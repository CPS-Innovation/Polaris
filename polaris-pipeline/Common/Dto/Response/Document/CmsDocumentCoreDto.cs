namespace Common.Dto.Response.Document
{
    public class CmsDocumentCoreDto
    {
        public CmsDocumentCoreDto()
        {
        }

        public long DocumentId { get; set; }

        public long VersionId { get; set; }

        public string Path { get; set; } /* DO NOT LEAK */
    }
}