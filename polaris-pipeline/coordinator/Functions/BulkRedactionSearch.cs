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
    //private const string OrchestrationInstanceIdHeader = "orchestrationInstanceId";

    public BulkRedactionSearch(ILogger<BulkRedactionSearch> logger, IBulkRedactionSearchService bulkRedactionSearchService)
    {
        _logger = logger;
        _bulkRedactionSearchService = bulkRedactionSearchService;
    }

    [Function(nameof(BulkRedactionSearch))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.OcrSearch)] HttpRequest req, string caseUrn,
        int caseId, string materialId, long documentId, CancellationToken cancellationToken,
        [DurableClient] DurableTaskClient orchestrationClient)
    {
        var currentCorrelationId = req.Headers.GetCorrelationId();
        var cmsAuthValues = req.Headers.GetCmsAuthValues();
        //var searchText = req.Query[SearchTextHeader];

        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = caseUrn,
            CaseId = caseId,
            MaterialId = materialId,
            DocumentId = documentId,
            //SearchText = searchText,
            CmsAuthValues = cmsAuthValues,
            CorrelationId = currentCorrelationId,
        };

        var response = await _bulkRedactionSearchService.InitiateOrOrchestrateOcr(bulkRedactionSearchDto, orchestrationClient, cancellationToken);

        var statusCode = response.DocumentRefreshStatus switch
        {
            OrchestrationProviderStatus.Initiated => HttpStatusCode.Accepted,
            OrchestrationProviderStatus.Processing => HttpStatusCode.Accepted,
            OrchestrationProviderStatus.Completed => HttpStatusCode.OK,
            OrchestrationProviderStatus.NotStarted => HttpStatusCode.BadRequest,
            OrchestrationProviderStatus.Failed => HttpStatusCode.NotFound,
            _ => HttpStatusCode.OK
        };

        return new ObjectResult(response)
        {
            StatusCode = (int?)statusCode,
        };
    }



    [Function(nameof(GetBulkRedactionSearchStatus))]
    public async Task<IActionResult> GetBulkRedactionSearchStatus(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.OcrSearch)] HttpRequest req, string caseUrn,
        int caseId, string materialId, long documentId, CancellationToken cancellationToken,
        [DurableClient] DurableTaskClient orchestrationClient)
    {
        var currentCorrelationId = req.Headers.GetCorrelationId();
        var cmsAuthValues = req.Headers.GetCmsAuthValues();
        var searchText = req.Query[SearchTextHeader];
        //var orchestrationInstanceId = req.Query[OrchestrationInstanceIdHeader];

        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = caseUrn,
            CaseId = caseId,
            MaterialId = materialId,
            DocumentId = documentId,
            SearchText = searchText,
            CmsAuthValues = cmsAuthValues,
            CorrelationId = currentCorrelationId,
        };

        var response = await _bulkRedactionSearchService.GetOcrSearchResults(bulkRedactionSearchDto, orchestrationClient, cancellationToken);

        var statusCode = response.DocumentRefreshStatus switch
        {
            OrchestrationProviderStatus.Initiated => HttpStatusCode.Accepted,
            OrchestrationProviderStatus.Processing => HttpStatusCode.Accepted,
            OrchestrationProviderStatus.Completed => HttpStatusCode.OK,
            OrchestrationProviderStatus.NotStarted => HttpStatusCode.BadRequest,
            OrchestrationProviderStatus.Failed => HttpStatusCode.NotFound,
            _ => HttpStatusCode.InternalServerError
        };

        return new ObjectResult(response)
        {
            StatusCode = (int?)statusCode,
        };
    }
}
