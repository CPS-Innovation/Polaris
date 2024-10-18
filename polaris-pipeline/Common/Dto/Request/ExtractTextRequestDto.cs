namespace Common.Dto.Request
{
    public class StoreCaseIndexesRequestDto
    {
        public StoreCaseIndexesRequestDto(string documentId, string blobName)
        {
            DocumentId = documentId;
        }

        public string DocumentId { get; set; }
    }
}