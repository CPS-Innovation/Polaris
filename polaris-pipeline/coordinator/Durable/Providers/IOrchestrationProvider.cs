using Common.Dto.Response;
using coordinator.Durable.Payloads;
using coordinator.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.DurableTask.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace coordinator.Durable.Providers;

public interface IOrchestrationProvider
{
    Task<List<int>> FindCaseInstancesByDateAsync(
        DurableTaskClient client,
        DateTime createdTimeTo,
        int batchSize);

    Task<bool> RefreshCaseAsync(
        DurableTaskClient client,
        Guid correlationId,
        int caseId,
        CasePayload casePayload,
        HttpRequest req);

    Task<DeleteCaseOrchestrationResult> DeleteCaseOrchestrationAsync(DurableTaskClient client, int caseId);
    Task<OrchestrationProviderStatuses> BulkSearchDocumentAsync(DurableTaskClient orchestrationClient, DocumentPayload documentPayload, CancellationToken cancellationToken = default);
}