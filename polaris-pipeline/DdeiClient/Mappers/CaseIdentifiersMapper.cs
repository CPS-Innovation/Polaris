using Common.Dto.Case;
using Ddei.Domain;
using DdeiClient.Mappers;

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