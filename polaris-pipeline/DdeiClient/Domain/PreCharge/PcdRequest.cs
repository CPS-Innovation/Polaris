namespace Ddei.Domain.PreCharge
{
    public class PcdRequest
    {
        public int Id { get; set; }

        public string DecisionRequiredBy { get; set; }

        public string DecisionRequested { get; set; }

        public List<PcdCaseOutlineLine> CaseOutline { get; set; }

        public PcdComments Comments { get; set; }

        public List<PcdRequestSuspect> Suspects { get; set; }

    }
}