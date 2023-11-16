using Common.Dto.Document;
using Common.Dto.Response;
using Common.Mappers.Contracts;

namespace Common.Mappers;

public class DdeiCaseDocumentMapper : ICaseDocumentMapper<DdeiCaseDocumentResponse>
{
    public CmsDocumentDto Map(DdeiCaseDocumentResponse ddeiResponse)
    {
        return new CmsDocumentDto
        {
            DocumentId = ddeiResponse.Id.ToString(),
            VersionId = ddeiResponse.VersionId,
            Path = ddeiResponse.Path,
            FileName = ddeiResponse.OriginalFileName,
            PresentationTitle = ddeiResponse.PresentationTitle,
            MimeType = ddeiResponse.MimeType,
            FileExtension = ddeiResponse.FileExtension,
            CmsDocType = new DocumentTypeDto(ddeiResponse.DocumentType, ddeiResponse.DocumentTypeId, ddeiResponse.CmsDocCategory),
            DocumentDate = ddeiResponse.DocumentDate,
            CategoryListOrder = ddeiResponse.CategoryListOrder,
            IsOcrProcessed = ddeiResponse.IsOcrProcessed == true,
            IsDispatched = ddeiResponse.IsDispatched
        };
    }
}
