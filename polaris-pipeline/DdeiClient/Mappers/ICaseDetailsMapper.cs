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
        CaseDto MapCaseDetails((CaseSummaryDto Summary, IEnumerable<PcdRequestDto> PreChargeDecisionRequests, IEnumerable<DefendantAndChargesDto> DefendantsAndCharges) caseDetails);
        DefendantsAndChargesListDto MapDefendantsAndCharges(IEnumerable<DdeiCaseDefendantDto> defendants, int caseId, string etag);
        PcdRequestDto MapPreChargeDecisionRequest(DdeiPcdRequestDto pcdr);
        IEnumerable<PcdRequestCoreDto> MapCorePreChargeDecisionRequests(IEnumerable<DdeiPcdRequestCoreDto> pcdRequests);
        IEnumerable<PcdRequestDto> MapPreChargeDecisionRequests(IEnumerable<DdeiPcdRequestDto> pcdRequests);
        CaseSummaryDto Map(MdsCaseSummaryDto ddeiResult);
    }
}