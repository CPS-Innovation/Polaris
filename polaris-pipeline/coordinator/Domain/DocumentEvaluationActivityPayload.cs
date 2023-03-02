using System;
using System.Collections.Generic;
using Common.Domain.DocumentEvaluation;

namespace coordinator.Domain;

public class DocumentEvaluationActivityPayload : BasePipelinePayload
{
    // TODO - move over to PolarisDocumentId
    public DocumentEvaluationActivityPayload(string caseUrn, long caseId, Guid correlationId)
     : base(default, caseUrn, caseId, correlationId)
    {
        DocumentsToRemove = new List<DocumentToRemove>();
    }
    
    public List<DocumentToRemove> DocumentsToRemove { get; set; }
}
