using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Common.Dto.Response.Case
{
    public class DefendantAndChargesDto
    {
        public DefendantAndChargesDto()
        {
        }

        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonProperty("listOrder")]
        [JsonPropertyName("listOrder")]
        public int? ListOrder { get; set; }

        [JsonProperty("defendantDetails")]
        [JsonPropertyName("defendantDetails")]
        public DefendantDetailsDto DefendantDetails { get; set; }

        [JsonProperty("custodyTimeLimit")]
        [JsonPropertyName("custodyTimeLimit")]
        public CustodyTimeLimitDto CustodyTimeLimit { get; set; }

        [JsonProperty("charges")]
        [JsonPropertyName("charges")]
        public IEnumerable<ChargeDto> Charges { get; set; }

        [JsonProperty("proposedCharges")]
        [JsonPropertyName("proposedCharges")]
        public IEnumerable<ProposedChargeDto> ProposedCharges { get; set; }
    }
}
