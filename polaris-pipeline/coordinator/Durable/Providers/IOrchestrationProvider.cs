using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Dto.Response;
using coordinator.Durable.Payloads;
using Microsoft.AspNetCore.Http;
using Microsoft.DurableTask.Client;

namespace coordinator.Durable.Providers;

public interface IOrchestrationProvider
{
    Task<List<int>> FindCaseInstancesByDateAsync(DurableTaskClient client,
                                                 DateTime createdTimeTo,
                                                 int batchSize);

    Task<bool> RefreshCaseAsync(DurableTaskClient client,
                                               Guid correlationId,
                                               int caseId,
                                               CasePayload casePayload,
                                               HttpRequest req);

    Task<DeleteCaseOrchestrationResult> DeleteCaseOrchestrationAsync(DurableTaskClient client, int caseId);
}