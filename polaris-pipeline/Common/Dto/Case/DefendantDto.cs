using System.Collections.Generic;
using Newtonsoft.Json;

namespace Common.Dto.Case
{
    public class DefendantDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("listOrder")]
        public int? ListOrder { get; set; }

        [JsonProperty("defendantDetails")]
        public DefendantDetails DefendantDetails { get; set; }

        [JsonProperty("custodyTimeLimit")]
        public CustodyTimeLimitDto CustodyTimeLimit { get; set; }

        [JsonProperty("charges")]
        public IEnumerable<ChargeDto> Charges { get; set; }

        [JsonProperty("proposedCharges")]
        public IEnumerable<ProposedChargeDto> ProposedCharges { get; set; }
    }
}
