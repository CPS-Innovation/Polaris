using Newtonsoft.Json;

namespace Common.Domain.Entity
{
    public class CaseLogEntity
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("timestamp")]
        public string TimeStamp { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}