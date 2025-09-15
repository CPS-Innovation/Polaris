using System;
using Common.Dto.Request.Redaction;
using System.Collections.Generic;
using Common.Dto.Response;

namespace coordinator.Durable.Payloads;
public class BulkRedactionSearchStatusPayload
{
    public int CaseId { get; set; }
    public string DocumentId { get; set; }
    public long VersionId { get; set; }
    public string SearchText { get; set; }
    public BulkRedactionSearchStatus Status { get; set; }
    public DateTime? OcrDocumentGeneratedAt { get; set; }
    public DateTime? DocumentSearchCompletedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string FailureReason { get; set; }
    public IEnumerable<RedactionDefinitionDto> RedactionDefinitions { get; set; }
}