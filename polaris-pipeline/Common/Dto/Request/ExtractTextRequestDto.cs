using System.ComponentModel.DataAnnotations;

namespace Common.Dto.Request
{
    public class StoreCaseIndexesRequestDto
    {
        public StoreCaseIndexesRequestDto(string documentId, string blobName)
        {
            PolarisDocumentId = documentId;
            BlobName = blobName;
        }

        public string PolarisDocumentId { get; set; }

        [Required]
        public string BlobName { get; set; }
    }
}