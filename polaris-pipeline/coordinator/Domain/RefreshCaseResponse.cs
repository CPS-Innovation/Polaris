using Newtonsoft.Json;

namespace coordinator.Domain
{
	public class RefreshCaseResponse
	{
		[JsonProperty("trackerUrl")]
		public string TrackerUrl { get => "/tracker"; }
	}
}

