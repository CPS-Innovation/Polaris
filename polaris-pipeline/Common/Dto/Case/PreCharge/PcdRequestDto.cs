using Common.Dto.FeatureFlags;
using System.Collections.Generic;

namespace Common.Dto.Case.PreCharge
{
    public class PcdRequestDto
    {
        public int Id { get; set; }

        public string DecisionRequiredBy { get; set; }

        public string DecisionRequested { get; set; }

        public List<PcdCaseOutlineLineDto> CaseOutline { get; set; }

        public PcdCommentsDto Comments { get; set; }

        public List<PcdRequestSuspectDto> Suspects { get; set; }

        public PresentationFlagsDto PresentationFlags { get; set; }
    }
}