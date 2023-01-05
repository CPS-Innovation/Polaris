using Newtonsoft.Json;

namespace coordinator.Domain.Tracker
{
    public class Log
    {
        [JsonProperty("logType")]
        public string LogType { get; set; }

        [JsonProperty("timestamp")]
        public string TimeStamp { get; set; }

        [JsonProperty("documentId", NullValueHandling = NullValueHandling.Ignore)]
        public string DocumentId { get; set; }
    }
}