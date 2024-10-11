using System.Collections.Generic;

namespace Common.Dto.Case.PreCharge
{
    public class PcdRequestDto : PcdRequestCoreDto
    {
        public List<PcdCaseOutlineLineDto> CaseOutline { get; set; }

        public PcdCommentsDto Comments { get; set; }

        public List<PcdRequestSuspectDto> Suspects { get; set; }
    }
}