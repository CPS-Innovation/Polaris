using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using coordinator.Durable.Payloads;
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

    Task DeleteCaseAsync(IDurableOrchestrationClient client,
                         Guid correlationId,
                         string caseUrn,
                         int caseId,
                         bool checkForBlobProtection = true,
                         bool waitForIndexToSettle = true);
}