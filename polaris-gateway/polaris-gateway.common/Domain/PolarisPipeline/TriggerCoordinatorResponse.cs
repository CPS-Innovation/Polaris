using Newtonsoft.Json;

namespace PolarisGateway.Domain.PolarisPipeline
{
	public class TriggerCoordinatorResponse
	{
		[JsonProperty("trackerUrl")]
		public Uri TrackerUrl { get; set; }
	}
}

