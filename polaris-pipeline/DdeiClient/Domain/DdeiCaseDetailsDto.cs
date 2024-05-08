using Ddei.Domain.PreCharge;

namespace Ddei.Domain
{
    public class DdeiCaseDetailsDto
    {
        public DdeiCaseSummaryDto Summary { get; set; }

        public IEnumerable<DdeiCaseDefendantDto> Defendants { get; set; }

        public IEnumerable<DdeiWitnessDto> Witnesses { get; set; }

        public IEnumerable<DdeiActionPlanDto> ActionPlans { get; set; }

        public IEnumerable<DdeiPcdRequestDto> PreChargeDecisionRequests { get; set; }
    }
}