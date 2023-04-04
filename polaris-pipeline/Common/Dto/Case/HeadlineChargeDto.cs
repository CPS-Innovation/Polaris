using Newtonsoft.Json;

namespace Common.Dto.Case
{
    public class HeadlineChargeDto
    {
        [JsonProperty("charge")]
        public string Charge { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("nextHearingDate")]
        public string NextHearingDate { get; set; }
    }
}
