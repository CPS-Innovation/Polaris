using Common.Configuration;
using Common.Dto.Request;
using Common.Extensions;
using coordinator.Durable.Providers;
using coordinator.Enums;
using coordinator.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace coordinator.Functions;

public class BulkRedactionSearch
{
    private readonly ILogger<BulkRedactionSearch> _logger;
    private readonly IBulkRedactionSearchService _bulkRedactionSearchService;
    private const string SearchTextHeader = "SearchText";

    public BulkRedactionSearch(ILogger<BulkRedactionSearch> logger, IBulkRedactionSearchService bulkRedactionSearchService)
    {
        _logger = logger;
        _bulkRedactionSearchService = bulkRedactionSearchService;
    }

    [Function(nameof(BulkRedactionSearch))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.OcrSearch)] HttpRequest req, string caseUrn,
        int caseId, string documentId, long versionId, CancellationToken cancellationToken,
        [DurableClient] DurableTaskClient orchestrationClient)
    {
        var currentCorrelationId = req.Headers.GetCorrelationId();
        var cmsAuthValues = req.Headers.GetCmsAuthValues();
        var searchText = req.Query[SearchTextHeader];

        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = caseUrn,
            CaseId = caseId,
            DocumentId = documentId,
            VersionId = versionId,
            SearchText = searchText,
            CmsAuthValues = cmsAuthValues,
            CorrelationId = currentCorrelationId
        };

        var bulkRedactionSearchResponse = await _bulkRedactionSearchService.BulkRedactionSearchAsync(bulkRedactionSearchDto, orchestrationClient, cancellationToken);

        var statusCode = bulkRedactionSearchResponse.DocumentRefreshStatus switch
        {
            OrchestrationProviderStatus.Initiated => HttpStatusCode.Accepted,
            OrchestrationProviderStatus.Processing => HttpStatusCode.Locked,
            OrchestrationProviderStatus.Completed => HttpStatusCode.OK,
            OrchestrationProviderStatus.Failed when bulkRedactionSearchResponse.IsNotFound => HttpStatusCode.NotFound,
            OrchestrationProviderStatus.Failed => HttpStatusCode.InternalServerError,
            _ => HttpStatusCode.OK
        };

        return new ObjectResult(bulkRedactionSearchResponse)
        {
            StatusCode = (int?)statusCode
        };
    }
}