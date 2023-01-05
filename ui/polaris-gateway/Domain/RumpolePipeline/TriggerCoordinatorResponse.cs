using System;
using Newtonsoft.Json;

namespace RumpoleGateway.Domain.RumpolePipeline
{
	public class TriggerCoordinatorResponse
	{
		[JsonProperty("trackerUrl")]
		public Uri TrackerUrl { get; set; }
	}
}

