using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using coordinator.Domain;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Providers;

public interface IOrchestrationProvider
{
    Task<List<string>> FindCaseInstancesByDateAsync(DateTime createdTimeTo, Guid correlationId, int batchSize);
    
    Task<HttpResponseMessage> RefreshCaseAsync(IDurableOrchestrationClient orchestrationClient, Guid correlationId,
        string caseId, CaseOrchestrationPayload casePayload, HttpRequestMessage req);

    Task<HttpResponseMessage> DeleteCaseAsync(IDurableOrchestrationClient orchestrationClient, Guid correlationId,
        int caseId, bool checkForBlobProtection);
}