using System;

namespace coordinator.Durable.Payloads;

public class BulkRedactionSearchPayload
{
    public string CaseUrn { get; set; }
    public int CaseId {get; set;}
    public string DocumentId {get; set;}
    public long VersionId {get; set;}
    public string SearchText { get; set; }
    public string CmsAuthDetails { get; set; }
    public Guid CorrelationId { get; set; }
}