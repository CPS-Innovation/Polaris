using System;
using Common.ValueObjects;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Factories
{
	public interface ITextExtractorHttpRequestFactory
	{
		DurableHttpRequest Create(PolarisDocumentId polarisDocumentId, long cmsCaseId, string cmsDocumentId, long versionId, string blobName, Guid correlationId);
	}
}

