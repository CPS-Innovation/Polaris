using Common.Domain.DocumentExtraction;
using Common.Domain.Responses;
using Common.Mappers.Contracts;

namespace Common.Mappers;

public class DdeiCaseDocumentMapper : ICaseDocumentMapper<DdeiCaseDocumentResponse>
{
    public CmsCaseDocument Map(DdeiCaseDocumentResponse ddeiResponse)
    {
        return new CmsCaseDocument
        {
            DocumentId = ddeiResponse.Id.ToString(),
            VersionId = ddeiResponse.VersionId,
            FileName = ddeiResponse.OriginalFileName,
            MimeType = ddeiResponse.MimeType,
            FileExtension = ddeiResponse.FileExtension,
            CmsDocType = new CmsDocType(ddeiResponse.DocumentType, ddeiResponse.DocumentTypeId, ddeiResponse.CmsDocCategory),
            DocumentDate = ddeiResponse.DocumentDate
        };
    }
}
