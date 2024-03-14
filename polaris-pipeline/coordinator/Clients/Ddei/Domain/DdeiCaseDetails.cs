using System.Collections.Generic;

namespace coordinator.Clients.Ddei.Domain
{
    public class DdeiCaseDetails
    {
        public DdeiCaseSummary Summary { get; set; }

        public IEnumerable<DdeiCaseDefendant> Defendants { get; set; }

        public IEnumerable<DdeiWitness> Witnesses { get; set; }

        public IEnumerable<DdeiActionPlan> ActionPlans { get; set; }

        public IEnumerable<DdeiPcdRequest> PreChargeDecisionRequests { get; set; }
    }
}