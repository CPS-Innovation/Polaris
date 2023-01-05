using System.ComponentModel.DataAnnotations;
using Common.Validators;

namespace Common.Domain.Requests
{
    public class ExtractTextRequest
    {
        public ExtractTextRequest(long caseId, string documentId, long versionId, string blobName)
        {
            CaseId = caseId;
            DocumentId = documentId;
            VersionId = versionId;
            BlobName = blobName;
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