using Ddei.Domain.Response.Defendant;
using Ddei.Domain.Response.PreCharge;

namespace Ddei.Domain.Response
{
    public class MdsCaseDetailsDto
    {
        public IEnumerable<MdsCaseDefendantDto> Defendants { get; set; }

        public IEnumerable<MdsPcdRequestDto> PreChargeDecisionRequests { get; set; }
    }
}