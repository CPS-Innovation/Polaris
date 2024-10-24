namespace Ddei.Domain.Response.PreCharge
{
    public class DdeiPcdRequestDto : DdeiPcdRequestCoreDto
    {
        public List<DdeiPcdCaseOutlineLineDto> CaseOutline { get; set; }

        public DdeiPcdCommentsDto Comments { get; set; }

        public List<DdeiPcdRequestSuspectDto> Suspects { get; set; }

    }
}