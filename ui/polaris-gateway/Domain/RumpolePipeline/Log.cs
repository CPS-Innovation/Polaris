using Newtonsoft.Json;

namespace RumpoleGateway.Domain.RumpolePipeline
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

