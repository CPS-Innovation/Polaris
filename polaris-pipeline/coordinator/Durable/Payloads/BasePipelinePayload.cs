using Common.Validators;
using Common.ValueObjects;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace coordinator.Durable.Payloads;

public abstract class BasePipelinePayload
{
    protected BasePipelinePayload(string cmsCaseUrn, long cmsCaseId, Guid correlationId, PolarisDocumentId polarisDocumentId = null)
    {
        CmsCaseUrn = cmsCaseUrn;
        CmsCaseId = cmsCaseId;
        CorrelationId = correlationId;
        PolarisDocumentId = polarisDocumentId;
    }

    [Required]
    public string CmsCaseUrn { get; set; }

    [RequiredLongGreaterThanZero]
    public long CmsCaseId { get; set; }

    [Required]
    public Guid CorrelationId { get; set; }

    [JsonIgnore]
    public PolarisDocumentId PolarisDocumentId { get; set; }

    [JsonPropertyName("PolarisDocumentId")]
    public string PolarisDocumentIdValue
    {
        get
        {
            return PolarisDocumentId?.ToString();
        }
        set
        {
            PolarisDocumentId = new PolarisDocumentId(value);
        }
    }
}