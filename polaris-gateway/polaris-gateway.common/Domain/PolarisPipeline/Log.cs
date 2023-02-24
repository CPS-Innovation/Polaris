using Newtonsoft.Json;

namespace PolarisGateway.Domain.PolarisPipeline
{
	public class Log
	{
        [JsonProperty("logType")]
        public string LogType { get; set; }

        [JsonProperty("timestamp")]
        public string TimeStamp { get; set; }

        [JsonProperty("documentId")]
        public string DocumentId { get; set; }
    }
}

