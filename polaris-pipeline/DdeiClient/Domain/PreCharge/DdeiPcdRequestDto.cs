namespace Ddei.Domain.PreCharge
{
    public class DdeiPcdRequestDto
    {
        public int Id { get; set; }

        public string DecisionRequiredBy { get; set; }

        public string DecisionRequested { get; set; }

        public List<DdeiPcdCaseOutlineLineDto> CaseOutline { get; set; }

        public DdeiPcdCommentsDto Comments { get; set; }

        public List<DdeiPcdRequestSuspectDto> Suspects { get; set; }

    }
}