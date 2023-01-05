using System.Collections.Generic;

namespace RumpoleGateway.CaseDataImplementations.Tde.Domain
{
    public class CaseDetails
    {
        public CaseSummary Summary { get; set; }

        public IEnumerable<CaseDefendant> Defendants { get; set; }

        public IEnumerable<ActionPlan> ActionPlans { get; set; }

        public IEnumerable<PreChargeDecisionRequest> PreChargeDecisionRequests { get; set; }

    }
}
