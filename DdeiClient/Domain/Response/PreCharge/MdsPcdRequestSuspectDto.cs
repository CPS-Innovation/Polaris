namespace Ddei.Domain.Response.PreCharge
{
    public class MdsPcdRequestSuspectDto
    {
        public MdsPcdRequestSuspectDto()
        {
            ProposedCharges = [];
        }

        public string Surname { get; set; }

        public string FirstNames { get; set; }

        public string Dob { get; set; }

        public string BailConditions { get; set; }

        public string BailDate { get; set; }

        public string RemandStatus { get; set; }

        public List<MdsPcdProposedChargeDto> ProposedCharges { get; set; }
    }
}