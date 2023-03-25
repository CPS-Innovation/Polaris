using Ddei.Domain.PreCharge;

namespace Ddei.Domain
{
    public class CaseDetails
    {
        public CaseSummary Summary { get; set; }

        public IEnumerable<CaseDefendant> Defendants { get; set; }

        public IEnumerable<ActionPlan> ActionPlans { get; set; }

        public IEnumerable<PcdRequest> PreChargeDecisionRequests { get; set; }
    }
}