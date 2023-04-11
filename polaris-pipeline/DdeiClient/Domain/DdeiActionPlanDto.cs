namespace Ddei.Domain
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
        public IEnumerable<ActionPlanItemDto> ActionPlanItems { get; set; }
        public string HasDocuments { get; set; }
    }

    public class ActionPlanItemDto
    {
        public string CaseActionPlanId { get; set; }
        public string CategoryId { get; set; }
        public string Text { get; set; }
    }
}