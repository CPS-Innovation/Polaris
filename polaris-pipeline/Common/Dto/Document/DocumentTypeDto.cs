namespace Common.Dto.Document
{
    public class DocumentTypeDto
    {
        public static string UnknownDocumentType = "1029";

        public DocumentTypeDto() { }

        public DocumentTypeDto(string documentType, string documentTypeId, string documentCategory)
        {
            DocumentTypeId = documentTypeId ?? UnknownDocumentType;
            DocumentType = documentType;
            DocumentCategory = documentCategory;
        }

        public string DocumentTypeId { get; set; }

        public string DocumentType { get; set; }

        public string DocumentCategory { get; set; }
    }
}

