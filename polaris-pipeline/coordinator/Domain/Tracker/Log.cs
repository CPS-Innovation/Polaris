using Newtonsoft.Json;

namespace coordinator.Domain.Tracker
{
    public class Log
    {
        [JsonProperty("logType")]
        public string LogType { get; set; }

        [JsonProperty("timestamp")]
        public string TimeStamp { get; set; }

        [JsonProperty("cmsDocumentId", NullValueHandling = NullValueHandling.Ignore)]
        public string CmsDocumentId { get; set; }
    }
}