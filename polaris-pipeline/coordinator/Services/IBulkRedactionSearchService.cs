using Microsoft.DurableTask.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using coordinator.Domain;

namespace coordinator.Services;

public interface IBulkRedactionSearchService
{
    Task<BulkRedactionSearchResponse> BulkRedactionSearchAsync(string caseUrn, int caseId, string documentId,
        long versionId, string searchText, DurableTaskClient orchestrationClient, string cmsAuthValues, Guid correlationId,
        CancellationToken cancellationToken);
}