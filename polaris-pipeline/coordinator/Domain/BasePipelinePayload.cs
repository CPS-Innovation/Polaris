using Common.Validators;
using System;
using System.ComponentModel.DataAnnotations;

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

    [Required]
    public string CmsCaseUrn { get; set; }

    [RequiredLongGreaterThanZero]
    public long CmsCaseId { get; set; }

    [Required]
    public Guid CorrelationId { get; set; }

    public Guid PolarisDocumentId { get; init; }

}