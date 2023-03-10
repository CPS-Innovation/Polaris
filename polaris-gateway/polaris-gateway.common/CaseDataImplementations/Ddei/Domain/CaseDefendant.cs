
using System.Collections.Generic;

namespace PolarisGateway.CaseDataImplementations.Ddei.Domain
{
    public class CaseDefendant
    {
        public int Id { get; set; }
        public int? ListOrder { get; set; }
        public string Type { get; set; }
        public string FirstNames { get; set; }
        public string Surname { get; set; }
        public string Dob { get; set; }
        public string RemandStatus { get; set; }
        public bool Youth { get; set; }
        public CustodyTimeLimit CustodyTimeLimit { get; set; }

        public IEnumerable<Offence> Offences { get; set; }

        public NextHearing NextHearing { get; set; }
    }
}