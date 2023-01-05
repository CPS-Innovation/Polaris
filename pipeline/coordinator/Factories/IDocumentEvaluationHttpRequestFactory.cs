using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Domain.DocumentEvaluation;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Factories
{
	public interface IDocumentEvaluationHttpRequestFactory
	{
		Task<DurableHttpRequest> Create(string caseUrn, long caseId, List<DocumentToRemove> documentsToRemove, Guid correlationId);
	}
}

