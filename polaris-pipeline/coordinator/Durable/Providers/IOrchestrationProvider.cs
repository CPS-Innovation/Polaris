using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Dto.Response;
using coordinator.Durable.Payloads;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Durable.Providers;

public interface IOrchestrationProvider
{
    Task<List<int>> FindCaseInstancesByDateAsync(IDurableOrchestrationClient client,
                                                 DateTime createdTimeTo,
                                                 int batchSize);

    Task<bool> RefreshCaseAsync(IDurableOrchestrationClient client,
                                               Guid correlationId,
                                               string caseId,
                                               CaseOrchestrationPayload casePayload,
                                               HttpRequest req);

    Task<DeleteCaseOrchestrationResult> DeleteCaseOrchestrationAsync(IDurableOrchestrationClient client, int caseId);
}