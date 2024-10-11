using Common.Dto.Case;
using Ddei.Domain.Response;

namespace DdeiClient.Mappers
{
    public interface ICaseIdentifiersMapper
    {
        CaseIdentifiersDto MapCaseIdentifiers(DdeiCaseIdentifiersDto caseIdentifiers);
    }
}