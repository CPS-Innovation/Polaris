using System;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Factories
{
    public interface IGeneratePdfHttpRequestFactory
    {
        DurableHttpRequest Create(string caseUrn, long caseId, string documentCategory, string documentId, string fileName, long versionId, string cmsAuthValues, Guid correlationId);
    }
}

