using System;

namespace Common.Domain.DocumentEvaluation;

public class IncomingDocument
{
    public IncomingDocument(Guid polarisDocumentId, string documentId, long versionId, string originalFileName)
    {
        PolarisDocumentId = polarisDocumentId;
        DocumentId = documentId;
        VersionId = versionId;
        OriginalFileName = originalFileName;
    }

    public Guid PolarisDocumentId { get; init;  }  

    public string DocumentId { get; init; }
    
    public long VersionId { get; set; }
    
    public string OriginalFileName { get; init; }
}
