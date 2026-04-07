namespace Ddei.Domain.Response.PreCharge
{
    public class MdsPcdRequestDto : MdsPcdRequestCoreDto
    {
        public List<MdsPcdCaseOutlineLineDto> CaseOutline { get; set; }

        public MdsPcdCommentsDto Comments { get; set; }

        public List<MdsPcdRequestSuspectDto> Suspects { get; set; }

    }
}