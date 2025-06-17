using Common.Dto.Response.Case.PreCharge;
using System.Collections.Generic;

namespace Common.Dto.Response.Case
{
    public class CaseDetailsDto
    {
        public CaseSummaryDto Summary { get; set; }
        public IEnumerable<PcdRequestDto> PreChargeDecisionRequests { get; set; }
        public IEnumerable<DefendantAndChargesDto> DefendantsAndCharges { get; set; }
        public IEnumerable<WitnessDto> Witnesses { get; set; }
    }
}
