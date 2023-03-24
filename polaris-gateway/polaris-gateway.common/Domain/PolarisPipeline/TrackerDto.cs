using Common.Domain.Case;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PolarisGateway.Domain.PolarisPipeline
{
	public class TrackerDto
	{
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("versionId")]
        public string VersionId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public TrackerStatus Status { get; set; }

        [JsonProperty("documents")]
        public List<TrackerDocument> Documents { get; set; }

        [JsonProperty("logs")]
        public List<Log> Logs { get; set; }
    }
}

