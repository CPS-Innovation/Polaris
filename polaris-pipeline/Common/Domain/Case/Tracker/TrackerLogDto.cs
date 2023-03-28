using Newtonsoft.Json;

namespace Common.Domain.Case.Tracker
{
    public class TrackerLogDto
    {
        [JsonProperty("logType")]
        public string LogType { get; set; }

        [JsonProperty("timestamp")]
        public string TimeStamp { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("cmsDocumentId", NullValueHandling = NullValueHandling.Ignore)]
        public string CmsDocumentId { get; set; }
    }
}