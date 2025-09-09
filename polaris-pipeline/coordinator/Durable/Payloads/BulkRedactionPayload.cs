namespace coordinator.Durable.Payloads;

public class BulkRedactionPayload
{
    public string CaseUrn { get; set; }
    public int CaseId {get; set;}
    public string DocumentId {get; set;}
    public long VersionId {get; set;}
    public string SearchText { get; set; }
}