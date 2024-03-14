using System.Collections.Generic;

namespace coordinator.Clients.Ddei.Domain
{
    public class DdeiPcdRequest
    {
        public int Id { get; set; }

        public string DecisionRequiredBy { get; set; }

        public string DecisionRequested { get; set; }

        public List<DdeiPcdCaseOutlineLine> CaseOutline { get; set; }

        public DdeiPcdComments Comments { get; set; }

        public List<DdeiPcdRequestSuspect> Suspects { get; set; }

    }
}