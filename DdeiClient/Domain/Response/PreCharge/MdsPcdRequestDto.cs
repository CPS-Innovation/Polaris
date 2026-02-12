namespace Ddei.Domain.Response.PreCharge
{
    public class MdsPcdRequestDto : MdsPcdRequestCoreDto
    {
        public List<DdeiPcdCaseOutlineLineDto> CaseOutline { get; set; }

        public DdeiPcdCommentsDto Comments { get; set; }

        public List<DdeiPcdRequestSuspectDto> Suspects { get; set; }

    }
}