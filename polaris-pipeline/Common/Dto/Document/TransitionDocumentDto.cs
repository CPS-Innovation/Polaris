using Common.Dto.Document;
using Common.Dto.FeatureFlags;
using System;

namespace Common.Dto.Document;

public class TransitionDocumentDto
{
    public TransitionDocumentDto()
    {
        CmsDocType = new DocumentTypeDto();
    }
    public TransitionDocumentDto(
        Guid polarisDocumentId,
        string cmsDocumentId,
        long cmsVersionId,
        string originalFileName,
        string mimeType,
        string fileExtension,
        DocumentTypeDto cmsDocType,
        string createdDate)
    {
        PolarisDocumentId = polarisDocumentId;
        CmsDocumentId = cmsDocumentId;
        CmsVersionId = cmsVersionId;
        OriginalFileName = originalFileName;
        MimeType = mimeType;
        FileExtension = fileExtension;
        CmsDocType = cmsDocType;
        CreatedDate = createdDate;
    }

    public Guid PolarisDocumentId { get; init; }

    public string CmsDocumentId { get; init; }

    public long CmsVersionId { get; set; }

    public string OriginalFileName { get; init; }

    public string MimeType { get; init; }

    public string FileExtension { get; set; }

    public DocumentTypeDto CmsDocType { get; set; }

    public string CreatedDate { get; set; }

    public PresentationFlagsDto PresentationFlags { get; set; }
}
