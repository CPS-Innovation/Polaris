using System;

namespace coordinator.Domain;

public abstract class BasePipelinePayload
{
    protected BasePipelinePayload(string cmsCaseUrn, long cmsCaseId, Guid correlationId, Guid polarisDocumentId=default)
    {
        PolarisDocumentId = polarisDocumentId;
        CmsCaseUrn = cmsCaseUrn;
        CmsCaseId = cmsCaseId;
        CorrelationId = correlationId;
    }

    public string CmsCaseUrn { get; set; }
    
    public long CmsCaseId { get; set; }
    
    public Guid CorrelationId { get; set; }
    public Guid PolarisDocumentId { get; init; }

}