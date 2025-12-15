using Common.Dto.Request;
using coordinator.Domain;
using Microsoft.DurableTask.Client;
using System.Threading;
using System.Threading.Tasks;

namespace coordinator.Services;

public interface IBulkRedactionSearchService
{
    Task<BulkRedactionSearchResponse> BulkRedactionSearchAsync(BulkRedactionSearchDto bulkRedactionSearchDto, DurableTaskClient orchestrationClient, CancellationToken cancellationToken);
}