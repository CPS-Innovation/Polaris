using Newtonsoft.Json;

namespace PolarisGateway.Domain.DocumentExtraction
{
	public class Case
	{
		[JsonProperty("caseId")]
		public string CaseId { get; set; }

		[JsonProperty("caseDocuments")]
		public CaseDocument[] CaseDocuments { get; set; }
	}
}

