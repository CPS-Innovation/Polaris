using Common.Validators;
using System;
using System.ComponentModel.DataAnnotations;

namespace coordinator.Durable.Payloads;

public abstract class BasePipelinePayload
{
    protected BasePipelinePayload(string cmsCaseUrn, int cmsCaseId, Guid correlationId, string documentId = null)
    {
        CmsCaseUrn = cmsCaseUrn;
        CmsCaseId = cmsCaseId;
        CorrelationId = correlationId;
        DocumentId = documentId;
    }

    [Required]
    public string CmsCaseUrn { get; set; }

    [RequiredLongGreaterThanZero]
    public int CmsCaseId { get; set; }

    [Required]
    public Guid CorrelationId { get; set; }

    public string DocumentId { get; set; }
}