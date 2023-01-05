using Common.Domain.DocumentExtraction;
using Common.Domain.Responses;
using Common.Mappers.Contracts;

namespace Common.Mappers;

public class DdeiCaseDocumentMapper : ICaseDocumentMapper<DdeiCaseDocumentResponse>
{
    public CaseDocument Map(DdeiCaseDocumentResponse ddeiResponse)
    {
        return new CaseDocument(ddeiResponse.Id.ToString(), ddeiResponse.VersionId, ddeiResponse.OriginalFileName, ddeiResponse.DocumentType, 
            ddeiResponse.DocumentTypeId, ddeiResponse.CmsDocCategory);
    }
}
