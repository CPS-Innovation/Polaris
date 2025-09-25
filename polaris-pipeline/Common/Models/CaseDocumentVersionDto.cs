namespace Common.Models;

public class CaseDocumentVersionDto
{
    public string Urn { get; set; }
    public int CaseId { get; set; }
    public string DocumentId { get; set; }
    public long VersionId { get; set; }
}