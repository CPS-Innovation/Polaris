using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Common.Dto.Response.Case
{
    public class DefendantAndChargesDto
    {
        public DefendantAndChargesDto()
        {
        }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("listOrder")]
        public int? ListOrder { get; set; }

        [JsonPropertyName("defendantDetails")]
        public DefendantDetailsDto DefendantDetails { get; set; }

        [JsonPropertyName("custodyTimeLimit")]
        public CustodyTimeLimitDto CustodyTimeLimit { get; set; }

        [JsonPropertyName("charges")]
        public IEnumerable<ChargeDto> Charges { get; set; }

        [JsonPropertyName("proposedCharges")]
        public IEnumerable<ProposedChargeDto> ProposedCharges { get; set; }
    }
}
