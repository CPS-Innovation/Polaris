namespace Ddei.Domain
{
    public class DdeiCaseDefendantDto
    {
        public int Id { get; set; }
        public int? ListOrder { get; set; }
        public string Type { get; set; }
        public string FirstNames { get; set; }
        public string Surname { get; set; }
        public string Dob { get; set; }
        public string RemandStatus { get; set; }
        public bool Youth { get; set; }
        public DdeiCustodyTimeLimitDto CustodyTimeLimit { get; set; }

        public IEnumerable<DdeiOffenceDto> Offences { get; set; }

        public DdeiNextHearingDto NextHearing { get; set; }
    }
}