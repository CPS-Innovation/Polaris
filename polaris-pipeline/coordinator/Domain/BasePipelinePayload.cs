using System;

namespace coordinator.Domain;

public abstract class BasePipelinePayload
{
    protected BasePipelinePayload(string caseUrn, long caseId, Guid correlationId)
    {
        CaseUrn = caseUrn;
        CaseId = caseId;
        CorrelationId = correlationId;
    }

    public string CaseUrn { get; set; }
    
    public long CaseId { get; set; }
    
    public Guid CorrelationId { get; set; }

}