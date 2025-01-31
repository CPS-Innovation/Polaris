using System.Text.Json.Serialization;

namespace Common.Dto.Response.Case
{
    public class CustodyTimeLimitDto
    {
        public CustodyTimeLimitDto()
        {
        }

        [JsonPropertyName("expiryDate")]
        public string ExpiryDate { get; set; }

        [JsonPropertyName("expiryDays")]
        public int? ExpiryDays { get; set; }

        [JsonPropertyName("expiryIndicator")]
        public string ExpiryIndicator { get; set; }
    }
}