using System.Text.Json.Serialization;

namespace Common.Dto.Response.Case
{
    public class ChargeDto
    {
        public ChargeDto()
        {
        }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("listOrder")]
        public int? ListOrder { get; set; }

        [JsonPropertyName("isCharged")]
        public bool IsCharged { get; set; }

        [JsonPropertyName("nextHearingDate")]
        public string NextHearingDate { get; set; }

        [JsonPropertyName("earlyDate")]
        public string EarlyDate { get; set; }

        [JsonPropertyName("lateDate")]
        public string LateDate { get; set; }

        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("shortDescription")]
        public string ShortDescription { get; set; }

        [JsonPropertyName("longDescription")]
        public string LongDescription { get; set; }

        [JsonPropertyName("custodyTimeLimit")]
        public CustodyTimeLimitDto CustodyTimeLimit { get; set; }
    }
}
