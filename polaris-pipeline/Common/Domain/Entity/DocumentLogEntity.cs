using Newtonsoft.Json;

namespace Common.Dto.Tracker
{
    public class TrackerDocumentLogDto
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("timespan")]
        public float? Timespan { get; set; }

        [JsonProperty("timestamp")]
        public string TimeStamp { get; set; }
    }
}