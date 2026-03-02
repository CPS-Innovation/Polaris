using Ddei.Domain.Response.Defendant;
using Ddei.Domain.Response.PreCharge;

namespace Ddei.Domain.Response
{
    public class MdsCaseDetailsDto
    {
        // Summary of type DdeiCaseSummaryDto not used. DdeiCaseSummaryDto deleted...
        //public DdeiCaseSummaryDto Summary { get; set; }

        public IEnumerable<MdsCaseDefendantDto> Defendants { get; set; }

        public IEnumerable<MdsWitnessDto> Witnesses { get; set; }

        public IEnumerable<MdsPcdRequestDto> PreChargeDecisionRequests { get; set; }
    }
}