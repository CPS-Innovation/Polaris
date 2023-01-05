using Newtonsoft.Json;

namespace PolarisGateway.Domain.DocumentExtraction
{
	public class CmsDocType
	{
		[JsonProperty("code")]
		public string Code { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }
	}
}

