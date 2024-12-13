using System.Text.Json.Serialization;

namespace Common.Dto.Response.Case
{
    public class HeadlineChargeDto
    {
        [JsonPropertyName("charge")]
        public string Charge { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("earlyDate")]
        public string EarlyDate { get; set; }

        [JsonPropertyName("lateDate")]
        public string LateDate { get; set; }

        [JsonPropertyName("nextHearingDate")]
        public string NextHearingDate { get; set; }
    }
}
