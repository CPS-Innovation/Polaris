using Common.Dto.Case;
using coordinator.Clients.Ddei.Domain;

namespace coordinator.Clients.Ddei.Mappers
{
    public interface ICaseIdentifiersMapper
    {
        CaseIdentifiersDto MapCaseIdentifiers(DdeiCaseIdentifiersDto caseIdentifiers);
    }
}