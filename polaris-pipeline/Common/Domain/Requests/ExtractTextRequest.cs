using System;
using System.ComponentModel.DataAnnotations;
using Common.Validators;

namespace Common.Domain.Requests
{
    public class ExtractTextRequest
    {
        public ExtractTextRequest(Guid polarisDocumentId, long cmsCaseId, string cmsDocumentId, long versionId, string blobName)
        {
            PolarisDocumentId = polarisDocumentId;
            CmsCaseId = cmsCaseId;
            CmsDocumentId = cmsDocumentId;
            VersionId = versionId;
            BlobName = blobName;
        }

        [Required]
        public Guid PolarisDocumentId { get; set; }

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