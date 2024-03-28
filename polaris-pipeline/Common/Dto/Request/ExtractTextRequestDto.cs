using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Common.ValueObjects;

namespace Common.Dto.Request
{
    public class StoreCaseIndexesRequestDto
    {
        public StoreCaseIndexesRequestDto(PolarisDocumentId polarisDocumentId, string blobName)
        {
            PolarisDocumentId = polarisDocumentId;
            BlobName = blobName;
        }

        [JsonIgnore]
        public PolarisDocumentId PolarisDocumentId { get; set; }

        [JsonPropertyName("PolarisDocumentId")]
        public string PolarisDocumentIdValue
        {
            get
            {
                return PolarisDocumentId?.ToString();
            }
            set
            {
                PolarisDocumentId = new PolarisDocumentId(value);
            }
        }

        [Required]
        public string BlobName { get; set; }
    }
}