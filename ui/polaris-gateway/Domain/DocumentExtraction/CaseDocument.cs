using System;
using Newtonsoft.Json;

namespace RumpoleGateway.Domain.DocumentExtraction
{
	public class CaseDocument
	{
		[JsonProperty("documentId")]
		public string DocumentId { get; set; }

		[JsonProperty("fileName")]
		public string FileName { get; set; }

		[JsonProperty("createdDate")]
		public DateTime CreatedDate { get; set; }

		[JsonProperty("cmsDocType")]
		public CmsDocType CmsDocType { get; set; }
	}
}