using Common.Validators;
using System;
using System.ComponentModel.DataAnnotations;

namespace coordinator.Durable.Payloads;

public class CasePayload
{
    public CasePayload()
    {
    }

    public CasePayload(string urn, int caseId, string cmsAuthValues, Guid correlationId)
    {
        Urn = urn;
        CaseId = caseId;
        CmsAuthValues = cmsAuthValues;
        CorrelationId = correlationId;
    }

    [Required]
    public string Urn { get; set; }

    [RequiredLongGreaterThanZero]
    public int CaseId { get; set; }

    public string CmsAuthValues { get; set; }

    [Required]
    public Guid CorrelationId { get; set; }
}