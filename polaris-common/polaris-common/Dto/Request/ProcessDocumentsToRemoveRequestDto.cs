using System.ComponentModel.DataAnnotations;
using polaris_common.Dto.Document;
using polaris_common.Validators;

namespace polaris_common.Dto.Request;

public class ProcessDocumentsToRemoveRequestDto
{
    public ProcessDocumentsToRemoveRequestDto(string caseUrn, long caseId, List<DocumentVersionDto> documentsToRemove)
    {
        CaseUrn = caseUrn;
        CaseId = caseId;
        DocumentsToRemove = documentsToRemove;
    }

    [Required]
    public string CaseUrn { get; set; }

    [RequiredLongGreaterThanZero]
    public long CaseId { get; set; }

    [Required]
    public List<DocumentVersionDto> DocumentsToRemove { get; set; }
}
