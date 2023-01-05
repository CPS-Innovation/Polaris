using System.Collections.Generic;
using Newtonsoft.Json;

namespace RumpoleGateway.Domain.CaseData
{
    public class Defendant
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("listOrder")]
        public int? ListOrder { get; set; }

        [JsonProperty("defendantDetails")]
        public DefendantDetails DefendantDetails { get; set; }

        [JsonProperty("custodyTimeLimit")]
        public CustodyTimeLimit CustodyTimeLimit { get; set; }

        [JsonProperty("charges")]
        public IEnumerable<Charge> Charges { get; set; }

        [JsonProperty("proposedCharges")]
        public IEnumerable<ProposedCharge> ProposedCharges { get; set; }
    }
}
