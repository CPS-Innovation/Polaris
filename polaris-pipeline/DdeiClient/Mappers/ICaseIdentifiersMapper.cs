using Common.Dto.Case;
using Ddei.Domain;

namespace DdeiClient.Mappers
{
    public interface ICaseIdentifiersMapper
    {
        CaseIdentifiersDto MapCaseIdentifiers(DdeiCaseIdentifiersDto caseIdentifiers);
    }
}