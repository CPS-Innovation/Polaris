using System;
using System.Net.Http;
using System.Threading.Tasks;
using coordinator.Domain;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Providers;

public interface IOrchestrationProvider
{
    Task<string> FindCaseInstanceByDateAsync(DateTime createdTimeTo, Guid correlationId);
    
    Task<HttpResponseMessage> RefreshCaseAsync(IDurableOrchestrationClient orchestrationClient, Guid correlationId,
        string caseId, CaseOrchestrationPayload casePayload, HttpRequestMessage req);

    Task<HttpResponseMessage> DeleteCaseAsync(IDurableOrchestrationClient orchestrationClient, Guid correlationId,
        int caseId);

    Task<HttpResponseMessage> UpdateTrackerAsync(IDurableOrchestrationClient orchestrationClient, Guid correlationId,
        string caseId, CaseOrchestrationPayload casePayload, HttpRequestMessage req);
}