using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Common.Dto.Response.Case
{
    public class HeadlineChargeDto
    {
        [JsonProperty("charge")]
        [JsonPropertyName("charge")]
        public string Charge { get; set; }

        [JsonProperty("date")]
        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonProperty("earlyDate")]
        [JsonPropertyName("earlyDate")]
        public string EarlyDate { get; set; }

        [JsonProperty("lateDate")]
        [JsonPropertyName("lateDate")]
        public string LateDate { get; set; }

        [JsonProperty("nextHearingDate")]
        [JsonPropertyName("nextHearingDate")]
        public string NextHearingDate { get; set; }
    }
}
