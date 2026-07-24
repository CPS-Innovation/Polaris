using Common.Configuration;
using Common.Constants;
using Common.Dto.Request;
using Common.Extensions;
using coordinator.Durable.Providers;
using coordinator.Enums;
using coordinator.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace coordinator.Functions;

public class BulkRedactionSearchPost
{
    private readonly IBulkRedactionSearchService _bulkRedactionSearchService;

    public BulkRedactionSearchPost(IBulkRedactionSearchService bulkRedactionSearchService)
    {
        _bulkRedactionSearchService = bulkRedactionSearchService;
    }

    [Function(nameof(BulkRedactionSearchPost))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.OcrSearch)] HttpRequest req, string caseUrn,
        int caseId, string materialId, long documentId, CancellationToken cancellationToken,
        [DurableClient] DurableTaskClient orchestrationClient)
    {
        var currentCorrelationId = req.Headers.GetCorrelationId();
        var cmsAuthValues = req.Headers.GetCmsAuthValues();

        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = caseUrn,
            CaseId = caseId,
            MaterialId = materialId,
            DocumentId = documentId,
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
            _ => HttpStatusCode.InternalServerError
        };

        return new ObjectResult(response)
        {
            StatusCode = (int?)statusCode,
        };
    }
}
