using Common.Domain.DocumentExtraction;
using Common.Domain.Responses;
using Common.Mappers.Contracts;

namespace Common.Mappers;

public class DdeiCaseDocumentMapper : ICaseDocumentMapper<DdeiCaseDocumentResponse>
{
    public CmsCaseDocument Map(DdeiCaseDocumentResponse ddeiResponse)
    {
        return new CmsCaseDocument(ddeiResponse.Id.ToString(), ddeiResponse.VersionId, ddeiResponse.OriginalFileName, ddeiResponse.DocumentType, 
            ddeiResponse.DocumentTypeId, ddeiResponse.CmsDocCategory);
    }
}
