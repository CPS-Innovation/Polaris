using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Factories
{
	public interface ITextExtractorHttpRequestFactory
	{
		Task<DurableHttpRequest> Create(long caseId, string documentId, long versionId, string blobName, Guid correlationId);
	}
}

