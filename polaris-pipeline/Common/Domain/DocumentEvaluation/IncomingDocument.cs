namespace Common.Domain.DocumentEvaluation;

public class IncomingDocument
{
    public IncomingDocument(string documentId, long versionId, string originalFileName)
    {
        DocumentId = documentId;
        VersionId = versionId;
        OriginalFileName = originalFileName;
    }

    public string DocumentId { get; set; }
    
    public long VersionId { get; set; }
    
    public string OriginalFileName { get; set; }
}
