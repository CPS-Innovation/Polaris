using Newtonsoft.Json;

namespace polaris_common.Dto.Case
{
    public class CustodyTimeLimitDto
    {
        [JsonProperty("expiryDate")]
        public string ExpiryDate { get; set; }

        [JsonProperty("expiryDays")]
        public int? ExpiryDays { get; set; }

        [JsonProperty("expiryIndicator")]
        public string ExpiryIndicator { get; set; }
    }
}