using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Factories
{
	public interface IGeneratePdfHttpRequestFactory
	{
		Task<DurableHttpRequest> Create(string caseUrn, long caseId, string documentCategory, string documentId, string fileName, long versionId, string upstreamToken, Guid correlationId);
	}
}

