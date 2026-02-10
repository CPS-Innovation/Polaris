using Common.Dto.Response.Case;
using Ddei.Domain.Response;

namespace Ddei.Mappers
{
    public interface ICaseIdentifiersMapper
    {
        CaseIdentifiersDto MapCaseIdentifiers(MdsCaseIdentifiersDto caseIdentifiers);
        CaseIdentifiersDto MapCaseIdentifiers(DdeiCaseSummaryDto caseIdentifiers);
    }
}