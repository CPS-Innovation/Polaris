using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Common.Domain.DocumentEvaluation;
using Common.Validators;

namespace Common.Domain.Requests;

public class ProcessDocumentsToRemoveRequest
{
    public ProcessDocumentsToRemoveRequest(string caseUrn, long caseId, List<DocumentToRemove> documentsToRemove)
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
    public List<DocumentToRemove> DocumentsToRemove { get; set; }
}
