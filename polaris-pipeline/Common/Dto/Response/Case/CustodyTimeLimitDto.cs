using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Common.Dto.Response.Case
{
    public class CustodyTimeLimitDto
    {
        public CustodyTimeLimitDto()
        {
        }

        [JsonProperty("expiryDate")]
        [JsonPropertyName("expiryDate")]
        public string ExpiryDate { get; set; }

        [JsonProperty("expiryDays")]
        [JsonPropertyName("expiryDays")]
        public int? ExpiryDays { get; set; }

        [JsonProperty("expiryIndicator")]
        [JsonPropertyName("expiryIndicator")]
        public string ExpiryIndicator { get; set; }
    }
}