using Newtonsoft.Json;

namespace RumpoleGateway.Domain.DocumentExtraction
{
	public class Case
	{
		[JsonProperty("caseId")]
		public string CaseId { get; set; }

		[JsonProperty("caseDocuments")]
		public CaseDocument[] CaseDocuments { get; set; }
	}
}

