using Common.Dto.Request;
using coordinator.Domain;
using Microsoft.DurableTask.Client;
using System.Threading;
using System.Threading.Tasks;
using coordinator.Enums;

namespace coordinator.Services;

public interface IBulkRedactionSearchService
{
    Task<BulkRedactionSearchResponse> InitiateOrOrchestrateOcr(BulkRedactionSearchDto bulkRedactionSearchDto, DurableTaskClient orchestrationClient, CancellationToken cancellationToken);

    Task<BulkRedactionSearchResponse> GetOcrSearchResults(BulkRedactionSearchDto bulkRedactionSearchDto, DurableTaskClient orchestrationClient, CancellationToken cancellationToken);


}