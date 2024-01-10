namespace polaris_common.Dto.Case.PreCharge
{
    public class PcdRequestSuspectDto
    {
        public PcdRequestSuspectDto()
        {
            ProposedCharges = new List<PcdProposedChargeDto>();
        }

        public string Surname { get; set; }

        public string FirstNames { get; set; }

        public string Dob { get; set; }

        public string BailConditions { get; set; }

        public string BailDate { get; set; }

        public string RemandStatus { get; set; }

        public List<PcdProposedChargeDto> ProposedCharges { get; set; }
    }
}