using System.ComponentModel.DataAnnotations;

namespace Common.Dto.Request
{
    public class StoreCaseIndexesRequestDto
    {
        public StoreCaseIndexesRequestDto(string documentId, string blobName)
        {
            DocumentId = documentId;
            BlobName = blobName;
        }

        public string DocumentId { get; set; }

        [Required]
        public string BlobName { get; set; }
    }
}