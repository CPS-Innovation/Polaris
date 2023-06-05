using Newtonsoft.Json;

namespace Common.Dto.Tracker
{
    public class DocumentLogEntity
    {
        [JsonProperty("timestamp")]
        public string TimeStamp { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}