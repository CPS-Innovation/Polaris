// <copyright file="BulkRedactionSearchResults.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Functions;

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

public class BulkRedactionSearchResults(IBulkRedactionSearchService bulkRedactionSearchService)
{
    private const string SearchTextHeader = "SearchText";

    [Function(nameof(BulkRedactionSearchResults))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.OcrSearch)] HttpRequest req, string caseUrn,
        int caseId, string materialId, long documentId, CancellationToken cancellationToken,
        [DurableClient] DurableTaskClient orchestrationClient)
    {
        var currentCorrelationId = req.Headers.GetCorrelationId();
        var cmsAuthValues = req.Headers.GetCmsAuthValues();
        var searchText = req.Query[SearchTextHeader];

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

        var response = await bulkRedactionSearchService.GetOcrSearchResults(bulkRedactionSearchDto, orchestrationClient, cancellationToken);

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
