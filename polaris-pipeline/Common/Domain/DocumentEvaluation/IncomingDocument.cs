using Common.Domain.DocumentExtraction;
using System;

namespace Common.Domain.DocumentEvaluation;

public class IncomingDocument
{
    public IncomingDocument(Guid polarisDocumentId, string documentId, long versionId, string originalFileName, string mimeType, CmsDocType cmsDocType, string createdDate)
    {
        PolarisDocumentId = polarisDocumentId;
        DocumentId = documentId;
        VersionId = versionId;
        OriginalFileName = originalFileName;
        MimeType = mimeType;
        CmsDocType = cmsDocType;
        CreatedDate = createdDate;
    }

    public Guid PolarisDocumentId { get; init;  }  

    public string DocumentId { get; init; }
    
    public long VersionId { get; set; }
    
    public string OriginalFileName { get; init; }

    public string MimeType { get; init; }

    public CmsDocType CmsDocType { get; set; }

    public string CreatedDate { get; set; }
}
