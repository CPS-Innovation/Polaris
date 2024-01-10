using Newtonsoft.Json;

namespace Common.Dto.Case
{
    public class HeadlineChargeDto
    {
        [JsonProperty("charge")]
        public string Charge { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("earlyDate")]
        public string EarlyDate { get; set; }

        [JsonProperty("lateDate")]
        public string LateDate { get; set; }

        [JsonProperty("nextHearingDate")]
        public string NextHearingDate { get; set; }
    }
}
