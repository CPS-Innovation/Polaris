using Newtonsoft.Json;

namespace RumpoleGateway.Domain.CaseData
{
    public class HeadlineCharge
    {
        [JsonProperty("charge")]
        public string Charge { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("nextHearingDate")]
        public string NextHearingDate { get; set; }
    }
}
