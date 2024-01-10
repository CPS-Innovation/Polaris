using Newtonsoft.Json;

namespace polaris_common.Dto.Tracker
{
    public class CaseLogDto
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("timestamp")]
        public string TimeStamp { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}