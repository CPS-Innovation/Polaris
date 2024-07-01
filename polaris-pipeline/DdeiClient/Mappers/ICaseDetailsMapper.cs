using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Ddei.Domain;
using Ddei.Domain.PreCharge;

namespace DdeiClient.Mappers
{
    public interface ICaseDetailsMapper
    {
        CaseDto MapCaseDetails(DdeiCaseDetailsDto caseDetails);
        IEnumerable<DefendantAndChargesDto> MapDefendantsAndCharges(IEnumerable<DdeiCaseDefendantDto> defendants);
        PcdRequestDto MapPreChargeDecisionRequest(DdeiPcdRequestDto pcdr);
        IEnumerable<PcdRequestCoreDto> MapCorePreChargeDecisionRequests(IEnumerable<DdeiPcdRequestCoreDto> pcdRequests);
    }
}