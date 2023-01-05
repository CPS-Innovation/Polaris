using System.ComponentModel.DataAnnotations;
using Common.Validators;

namespace Common.Domain.Requests;

public class EvaluateDocumentRequest
{
    public EvaluateDocumentRequest(long caseId, string documentId, long versionId, string proposedBlobName)
    {
        CaseId = caseId;
        DocumentId = documentId;
        VersionId = versionId;
        ProposedBlobName = proposedBlobName;
    }
    
    [RequiredLongGreaterThanZero]
    public long CaseId { get; set; }

    [Required]
    public string DocumentId { get; set; }
        
    [RequiredLongGreaterThanZero] 
    public long VersionId { get; set; }
    
    [Required]
    public string ProposedBlobName { get; set; }
}
