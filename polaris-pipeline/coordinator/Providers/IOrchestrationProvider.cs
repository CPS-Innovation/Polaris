using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Dto.Response;
using coordinator.Domain;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Providers;

public interface IOrchestrationProvider
{
    Task<List<int>> FindCaseInstancesByDateAsync(IDurableOrchestrationClient client,
                                                 DateTime createdTimeTo,
                                                 int batchSize);

    Task<HttpResponseMessage> RefreshCaseAsync(IDurableOrchestrationClient client,
                                               Guid correlationId,
                                               string caseId,
                                               CaseOrchestrationPayload casePayload,
                                               HttpRequestMessage req);

    Task<DeleteCaseOrchestrationResult> DeleteCaseOrchestrationAsync(IDurableOrchestrationClient client, int caseId);
}