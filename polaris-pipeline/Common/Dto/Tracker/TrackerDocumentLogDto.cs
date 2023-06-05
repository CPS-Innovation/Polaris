using Newtonsoft.Json;

namespace Common.Dto.Tracker
{
    public class TrackerDocumentLogDto
    {
        [JsonProperty("timestamp")]
        public string TimeStamp { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}