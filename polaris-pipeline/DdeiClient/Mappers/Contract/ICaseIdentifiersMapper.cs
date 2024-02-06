using Common.Dto.Case;
using Ddei.Domain;

namespace DdeiClient.Mappers.Contract
{
    public interface ICaseIdentifiersMapper
    {
        CaseIdentifiersDto MapCaseIdentifiers(DdeiCaseIdentifiersDto caseIdentifiers);
    }
}