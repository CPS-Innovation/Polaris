using Common.Dto.Response.Case;
using Ddei.Domain.Response;

namespace Ddei.Mappers;

public class CaseIdentifiersMapper : ICaseIdentifiersMapper
{
    public CaseIdentifiersDto MapCaseIdentifiers(MdsCaseIdentifiersDto caseIdentifiers)
    {
        return new CaseIdentifiersDto
        {
            Id = caseIdentifiers.Id,
            Urn = caseIdentifiers.Urn,
            UrnRoot = caseIdentifiers.UrnRoot,
        };
    }

    public CaseIdentifiersDto MapCaseIdentifiers(DdeiCaseSummaryDto caseIdentifiers)
    {
        return new CaseIdentifiersDto
        {
            Id = caseIdentifiers.Id,
            Urn = caseIdentifiers.Urn,
            UrnRoot = caseIdentifiers.Urn
                    .Split("/")
                    .FirstOrDefault(),
        };
    }
}