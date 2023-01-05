using System.Diagnostics.CodeAnalysis;
using Common.Domain.DocumentExtraction;
using Common.Mappers.Contracts;

namespace Common.Mappers;

[ExcludeFromCodeCoverage]
public class CgiDocumentMapper : ICaseDocumentMapper<Case>
{
    public CaseDocument Map(Case item)
    {
        return null; //for now
    }
}
