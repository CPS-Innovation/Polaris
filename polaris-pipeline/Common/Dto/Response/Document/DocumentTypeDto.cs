namespace Common.Dto.Response.Document
{
    public class DocumentTypeDto
    {
        public const int UnknownDocumentType = 1029;

        public DocumentTypeDto() { }

        public DocumentTypeDto(string documentType, int? documentTypeId, string documentCategory)
        {
            DocumentTypeId = documentTypeId ?? UnknownDocumentType;
            DocumentType = documentType;
            DocumentCategory = documentCategory;
        }

        public int? DocumentTypeId { get; set; }

        public string DocumentType { get; set; }

        public string DocumentCategory { get; set; }
    }
}

