using Ddei.Domain.Response.Defendant;
using Ddei.Domain.Response.PreCharge;

namespace Ddei.Domain.Response
{
    public class DdeiCaseDetailsDto
    {
        public DdeiCaseSummaryDto Summary { get; set; }

        public IEnumerable<DdeiCaseDefendantDto> Defendants { get; set; }

        public IEnumerable<DdeiWitnessDto> Witnesses { get; set; }

        public IEnumerable<MdsPcdRequestDto> PreChargeDecisionRequests { get; set; }
    }
}