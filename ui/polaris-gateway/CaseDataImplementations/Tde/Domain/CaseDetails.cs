using System.Collections.Generic;

namespace PolarisGateway.CaseDataImplementations.Ddei.Domain
{
    public class CaseDetails
    {
        public CaseSummary Summary { get; set; }

        public IEnumerable<CaseDefendant> Defendants { get; set; }

        public IEnumerable<ActionPlan> ActionPlans { get; set; }

        public IEnumerable<PreChargeDecisionRequest> PreChargeDecisionRequests { get; set; }

    }
}
