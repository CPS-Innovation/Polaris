namespace RumpoleGateway.CaseDataImplementations.Tde.Domain
{
    public class CaseSummary
    {
        public string Urn { get; set; }
        public int Id { get; set; }
        public int NumberOfDefendants { get; set; }
        public string LeadDefendantFirstNames { get; set; }
        public string LeadDefendantSurname { get; set; }
        public string LeadDefendantType { get; set; }
        public bool Deleted { get; set; }
        public bool Finalised { get; set; }
        public string NextHearingDate { get; set; }
        public string NextHearingType { get; set; }
        public string NextHearingTypeCode { get; set; }
        public string NextHearingVenue { get; set; }
        public string NextHearingVenueCode { get; set; }
        public bool CtlActive { get; set; }
        public string EarliestCtlDate { get; set; }
        public Locking Locking { get; set; }
    }

    public class Locking
    {
        public string Application { get; set; }
        public string BySurname { get; set; }
        public string ByFirstNames { get; set; }
        public string Locked { get; set; }
        public string Since { get; set; }
    }
}