using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Ddei.Domain.Response;
using Ddei.Domain.Response.Defendant;
using Ddei.Domain.Response.PreCharge;

namespace Ddei.Mappers
{
    public interface ICaseDetailsMapper
    {
        CaseDto MapCaseDetails(DdeiCaseDetailsDto caseDetails);
        IEnumerable<DefendantAndChargesDto> MapDefendantsAndCharges(IEnumerable<DdeiCaseDefendantDto> defendants, string etag);
        PcdRequestDto MapPreChargeDecisionRequest(DdeiPcdRequestDto pcdr, string etag);
        IEnumerable<PcdRequestCoreDto> MapCorePreChargeDecisionRequests(IEnumerable<DdeiPcdRequestCoreDto> pcdRequests);
    }
}