
namespace RumpoleGateway.CaseDataImplementations.Tde.Domain
{
    public class Offence
    {
        public int Id { get; set; }
        public int? ListOrder { get; set; }
        public string Code { get; set; }
        public string Type { get; set; }
        public string Active { get; set; }
        public string Description { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string LatestPlea { get; set; }
        public string LatestVerdict { get; set; }
        public string DisposedReason { get; set; }
        public string LastHearingOutcome { get; set; }

        public CustodyTimeLimit CustodyTimeLimit { get; set; }
    }
}