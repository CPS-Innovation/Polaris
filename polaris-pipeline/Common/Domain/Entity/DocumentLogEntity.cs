using Newtonsoft.Json;

namespace Common.Domain.Entity
{
    public class DocumentLogEntity
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("timestamp")]
        public string TimeStamp { get; set; }

        [JsonProperty("timespan")]
        public float? Timespan { get; set; }
    }
}