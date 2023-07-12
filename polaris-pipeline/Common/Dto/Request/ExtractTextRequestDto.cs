using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Common.Validators;
using Common.ValueObjects;

namespace Common.Dto.Request
{
    public class ExtractTextRequestDto
    {
        public ExtractTextRequestDto(PolarisDocumentId polarisDocumentId, long cmsCaseId, string cmsDocumentId, long versionId, string blobName)
        {
            PolarisDocumentId = polarisDocumentId;
            CaseId = cmsCaseId;
            DocumentId = cmsDocumentId;
            VersionId = versionId;
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

        [RequiredLongGreaterThanZero]
        public long CaseId { get; set; }

        [Required]
        public string DocumentId { get; set; }

        [RequiredLongGreaterThanZero]
        public long VersionId { get; set; }

        [Required]
        public string BlobName { get; set; }
    }
}