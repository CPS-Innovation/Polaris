using System.Collections.Generic;

namespace coordinator.Clients.Ddei.Domain
{
    public class DdeiCaseDefendant
    {
        public int Id { get; set; }
        public int? ListOrder { get; set; }
        public string Type { get; set; }
        public string FirstNames { get; set; }
        public string Surname { get; set; }
        public string Dob { get; set; }
        public string RemandStatus { get; set; }
        public bool Youth { get; set; }
        public DdeiCustodyTimeLimit CustodyTimeLimit { get; set; }

        public IEnumerable<DdeiOffence> Offences { get; set; }

        public DdeiNextHearing NextHearing { get; set; }
    }
}