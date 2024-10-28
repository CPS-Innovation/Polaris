namespace Common.Dto.Response.Document
{
    public class DocumentReclassifiedResultDto
    {
        public long DocumentId { get; set; }
        public int DocumentTypeId { get; set; }
        public int OriginalDocumentTypeId { get; set; }
        public string ReclassificationType { get; set; }
        public bool DocumentRenamed { get; set; }
        public string DocumentRenamedOperationName { get; set; }
    }
}