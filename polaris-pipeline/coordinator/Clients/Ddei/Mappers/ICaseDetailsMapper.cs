using Common.Dto.Case;
using coordinator.Clients.Ddei.Domain;

namespace coordinator.Clients.Ddei.Mappers
{
    public interface ICaseDetailsMapper
    {
        CaseDto MapCaseDetails(DdeiCaseDetails caseDetails);
    }
}