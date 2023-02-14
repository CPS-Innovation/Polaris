using System;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Factories
{
	public interface ITextExtractorHttpRequestFactory
	{
		DurableHttpRequest Create(long caseId, string documentId, long versionId, string blobName, Guid correlationId);
	}
}

