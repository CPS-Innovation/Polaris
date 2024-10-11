using Common.Dto.Response.Case;
using Ddei.Domain.Response;

namespace Ddei.Mappers;

public class CaseIdentifiersMapper : ICaseIdentifiersMapper
{
    public CaseIdentifiersDto MapCaseIdentifiers(DdeiCaseIdentifiersDto caseIdentifiers)
    {
        return new CaseIdentifiersDto
        {
            Id = caseIdentifiers.Id,
            Urn = caseIdentifiers.Urn,
            UrnRoot = caseIdentifiers.UrnRoot,
        };
    }
}