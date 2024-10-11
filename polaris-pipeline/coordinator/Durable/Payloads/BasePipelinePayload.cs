using Common.Validators;
using System;
using System.ComponentModel.DataAnnotations;

namespace coordinator.Durable.Payloads;

public abstract class BasePipelinePayload
{
    protected BasePipelinePayload(string urn, int caseId, Guid correlationId, string documentId = null)
    {
        Urn = urn;
        CaseId = caseId;
        CorrelationId = correlationId;
        DocumentId = documentId;
    }

    [Required]
    public string Urn { get; set; }

    [RequiredLongGreaterThanZero]
    public int CaseId { get; set; }

    [Required]
    public Guid CorrelationId { get; set; }

    public string DocumentId { get; set; }
}