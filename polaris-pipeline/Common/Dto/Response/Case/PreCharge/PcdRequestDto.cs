using System.Collections.Generic;

namespace Common.Dto.Response.Case.PreCharge
{
    public class PcdRequestDto : PcdRequestCoreDto
    {
        public long VersionId { get; set; }
        public List<PcdCaseOutlineLineDto> CaseOutline { get; set; }

        public PcdCommentsDto Comments { get; set; }

        public List<PcdRequestSuspectDto> Suspects { get; set; }
    }
}