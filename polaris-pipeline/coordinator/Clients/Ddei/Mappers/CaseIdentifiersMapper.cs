using Common.Dto.Case;
using coordinator.Clients.Ddei.Domain;

namespace coordinator.Clients.Ddei.Mappers;

public class CaseIdentifiersMapper : ICaseIdentifiersMapper
{
    public CaseIdentifiersDto MapCaseIdentifiers(DdeiCaseIdentifiers caseIdentifiers)
    {
        return new CaseIdentifiersDto
        {
            Id = caseIdentifiers.Id,
            Urn = caseIdentifiers.Urn,
            UrnRoot = caseIdentifiers.UrnRoot,
        };
    }
}