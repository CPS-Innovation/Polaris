using Newtonsoft.Json;

namespace PolarisAuthHandover.Domain.CaseData
{
    public class CustodyTimeLimit
    {
        [JsonProperty("expiryDate")]
        public string ExpiryDate { get; set; }

        [JsonProperty("expiryDays")]
        public int? ExpiryDays { get; set; }

        [JsonProperty("expiryIndicator")]
        public string ExpiryIndicator { get; set; }
    }
}