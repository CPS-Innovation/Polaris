using polaris_common.Constants;

namespace polaris_common.Dto.Document
{
    public class DocumentTypeDto
    {
        public DocumentTypeDto() { }

        public DocumentTypeDto(string documentType, string documentTypeId, string documentCategory)
        {
            DocumentTypeId = documentTypeId ?? MiscCategories.UnknownDocumentType;
            DocumentType = documentType;
            DocumentCategory = documentCategory;
        }

        public string DocumentTypeId { get; set; }

        public string DocumentType { get; set; }

        public string DocumentCategory { get; set; }
    }
}

