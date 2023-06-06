using Newtonsoft.Json;

namespace Common.Dto.Tracker
{
    public class DocumentLogEntity
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("timestamp")]
        public string TimeStamp { get; set; }

        [JsonProperty("timespanSeconds")]
        public float? TimespanSeconds { get; set; }
    }
}