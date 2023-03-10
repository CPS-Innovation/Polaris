using System;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Factories
{
	public interface ITextExtractorHttpRequestFactory
	{
		DurableHttpRequest Create(Guid polarisDocumentId, long cmsCaseId, string cmsDocumentId, long versionId, string blobName, Guid correlationId);
	}
}

