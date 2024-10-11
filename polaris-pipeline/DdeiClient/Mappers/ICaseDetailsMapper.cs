using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Ddei.Domain.Response;
using Ddei.Domain.Response.Defendant;
using Ddei.Domain.Response.PreCharge;

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