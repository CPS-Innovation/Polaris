using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Common.Dto.Response.Document;
using Common.Validators;

namespace Common.Dto.Request;

public class ProcessDocumentsToRemoveRequestDto
{
    public ProcessDocumentsToRemoveRequestDto(string caseUrn, int caseId, List<DocumentVersionDto> documentsToRemove)
    {
        CaseUrn = caseUrn;
        CaseId = caseId;
        DocumentsToRemove = documentsToRemove;
    }

    [Required]
    public string CaseUrn { get; set; }

    [RequiredLongGreaterThanZero]
    public int CaseId { get; set; }

    [Required]
    public List<DocumentVersionDto> DocumentsToRemove { get; set; }
}
