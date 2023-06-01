using Common.Dto.Document;
using Common.Dto.Response;
using Common.Mappers.Contracts;

namespace Common.Mappers;

public class DdeiCaseDocumentMapper : ICaseDocumentMapper<DdeiCaseDocumentResponse>
{
    public DocumentDto Map(DdeiCaseDocumentResponse ddeiResponse)
    {
        return new DocumentDto
        {
            DocumentId = ddeiResponse.Id.ToString(),
            VersionId = ddeiResponse.VersionId,
            FileName = ddeiResponse.OriginalFileName,
            PresentationTitle = ddeiResponse.PresentationTitle,
            MimeType = ddeiResponse.MimeType,
            FileExtension = ddeiResponse.FileExtension,
            CmsDocType = new DocumentTypeDto(ddeiResponse.DocumentType, ddeiResponse.DocumentTypeId, ddeiResponse.CmsDocCategory),
            DocumentDate = ddeiResponse.DocumentDate,
            IsOcrProcessed = ddeiResponse.IsOcrProcessed == true
        };
    }
}
