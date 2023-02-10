using System.Diagnostics.CodeAnalysis;
using Common.Domain.DocumentExtraction;
using Common.Mappers.Contracts;

namespace Common.Mappers;

[ExcludeFromCodeCoverage]
public class CgiDocumentMapper : ICaseDocumentMapper<CmsCase>
{
    public CmsCaseDocument Map(CmsCase item)
    {
        return null; //for now
    }
}
