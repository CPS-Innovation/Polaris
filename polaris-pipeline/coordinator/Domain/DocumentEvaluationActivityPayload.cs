using System;
using System.Collections.Generic;
using Common.Domain.DocumentEvaluation;

namespace coordinator.Domain;

public class DocumentEvaluationActivityPayload : BasePipelinePayload
{
    public DocumentEvaluationActivityPayload(string caseUrn, long caseId, Guid correlationId)
     : base(caseUrn, caseId, correlationId)
    {
        DocumentsToRemove = new List<DocumentToRemove>();
    }
    
    public List<DocumentToRemove> DocumentsToRemove { get; set; }
}
