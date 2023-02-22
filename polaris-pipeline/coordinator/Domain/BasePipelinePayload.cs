using System;

namespace coordinator.Domain;

public abstract class BasePipelinePayload
{
    protected BasePipelinePayload(Guid polarisDocumentId, string cmsCaseUrn, long cmsCaseId, Guid correlationId)
    {
        PolarisDocumentId = polarisDocumentId;
        CmsCaseUrn = cmsCaseUrn;
        CmsCaseId = cmsCaseId;
        CorrelationId = correlationId;
    }

    public Guid PolarisDocumentId { get; init; }

    public string CmsCaseUrn { get; set; }
    
    public long CmsCaseId { get; set; }
    
    public Guid CorrelationId { get; set; }
}