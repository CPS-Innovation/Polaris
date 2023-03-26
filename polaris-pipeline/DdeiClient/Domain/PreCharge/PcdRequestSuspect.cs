namespace Ddei.Domain.PreCharge
{
    public class PcdRequestSuspect
    {
        public PcdRequestSuspect()
        {
            ProposedCharges = new List<PcdProposedCharge>();
        }

        public string Surname { get; set; }

        public string FirstNames { get; set; }

        public string Dob { get; set; }

        public string BailConditions { get; set; }

        public string BailDate { get; set; }

        public string RemandStatus { get; set; }

        public List<PcdProposedCharge> ProposedCharges { get; set; }
    }
}