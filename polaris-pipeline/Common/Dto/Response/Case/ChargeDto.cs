using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Common.Dto.Response.Case
{
    public class ChargeDto
    {
        public ChargeDto()
        {
        }

        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonProperty("listOrder")]
        [JsonPropertyName("listOrder")]
        public int? ListOrder { get; set; }

        [JsonProperty("isCharged")]
        [JsonPropertyName("isCharged")]
        public bool IsCharged { get; set; }

        [JsonProperty("nextHearingDate")]
        [JsonPropertyName("nextHearingDate")]
        public string NextHearingDate { get; set; }

        [JsonProperty("earlyDate")]
        [JsonPropertyName("earlyDate")]
        public string EarlyDate { get; set; }

        [JsonProperty("lateDate")]
        [JsonPropertyName("lateDate")]
        public string LateDate { get; set; }

        [JsonProperty("code")]
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonProperty("shortDescription")]
        [JsonPropertyName("shortDescription")]
        public string ShortDescription { get; set; }

        [JsonProperty("longDescription")]
        [JsonPropertyName("longDescription")]
        public string LongDescription { get; set; }

        [JsonProperty("custodyTimeLimit")]
        [JsonPropertyName("custodyTimeLimit")]
        public CustodyTimeLimitDto CustodyTimeLimit { get; set; }
    }
}
