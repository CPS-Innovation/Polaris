using System;
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
            CmsCaseId = cmsCaseId;
            CmsDocumentId = cmsDocumentId;
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
        public long CmsCaseId { get; set; }

        [Required]
        public string CmsDocumentId { get; set; }

        [RequiredLongGreaterThanZero]
        public long VersionId { get; set; }

        [Required]
        public string BlobName { get; set; }
    }
}