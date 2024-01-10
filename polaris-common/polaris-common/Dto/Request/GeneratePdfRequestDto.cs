using System.ComponentModel.DataAnnotations;
using polaris_common.Validators;

namespace polaris_common.Dto.Request
{
    public class GeneratePdfRequestDto
    {
        public GeneratePdfRequestDto(string caseUrn, long caseId, string documentCategory, string documentId, string fileName, long versionId)
        {
            CaseUrn = caseUrn;
            CaseId = caseId;
            DocumentCategory = documentCategory;
            DocumentId = documentId;
            FileName = fileName;
            VersionId = versionId;
        }

        [Required]
        public string CaseUrn { get; set; }

        [RequiredLongGreaterThanZero]
        public long CaseId { get; set; }

        [Required]
        public string DocumentId { get; set; }

        public string DocumentCategory { get; set; }

        [Required]
        [RegularExpression(@"^.+\.[A-Za-z]{3,4}$")]
        public string FileName { get; set; }

        [RequiredLongGreaterThanZero]
        public long VersionId { get; set; }
    }
}