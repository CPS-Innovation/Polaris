namespace Ddei.Domain.Response.ActionPlan
{
    public class DdeiActionPlanDto
    {
        public int Id { get; set; }

        public string ActionType { get; set; }
        public string RequiredByDate { get; set; }
        public string ContestedIssue { get; set; }
        public string Status { get; set; }
        public string SentDate { get; set; }
        public string Defendant { get; set; }
        public string ChaserDate { get; set; }
        public string AllDefendantsFlag { get; set; }
        public string DefendantId { get; set; }
        public IEnumerable<DdeiActionPlanItemDto> ActionPlanItems { get; set; }
        public string HasDocuments { get; set; }
    }
}