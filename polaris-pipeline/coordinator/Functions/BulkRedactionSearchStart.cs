// <copyright file="BulkRedactionSearchStart.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Functions;

using Common.Configuration;
using Common.Constants;
using Common.Domain.Document;
using Common.Dto.Request;
using Common.Extensions;
using Common.Telemetry;
using coordinator.Durable.Providers;
using coordinator.Enums;
using coordinator.Services;
using coordinator.TelemetryEvents;
using Ddei.Factories;
using DdeiClient.Services.CaseUrnResolver;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class BulkRedactionSearchStart(IBulkRedactionSearchService bulkRedactionSearchService, ICaseUrnResolver caseUrnResolver, ILogger<BulkRedactionSearchStart> logger)
{
    [Function(nameof(BulkRedactionSearchStart))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.OcrSearch)] HttpRequest req,
        int caseId,
        string materialId,
        long documentId,
        CancellationToken cancellationToken,
        [DurableClient] DurableTaskClient orchestrationClient)
    {
        Guid currentCorrelationId = req.Headers.GetCorrelationId();
        CmsAuthValues cmsAuthValues = req.BuildCmsAuthValues();

        var caseUrn = await caseUrnResolver.ResolveCaseUrnAsync(caseId, cmsAuthValues, cancellationToken);

        var qualifiedMaterialId = DocumentNature.NormalizeMaterialId(materialId);
        if (materialId.StartsWith("CMS-", StringComparison.Ordinal))
        {
            logger.TrackEvent(new CmsPrefixUsageEvent(currentCorrelationId, caseId, documentId.ToString(), nameof(BulkRedactionSearchStart)));
        }

        var bulkRedactionSearchDto = new BulkRedactionSearchDto
        {
            Urn = caseUrn,
            CaseId = caseId,
            MaterialId = qualifiedMaterialId,
            DocumentId = documentId,
            CmsAuthValues = cmsAuthValues.CmsAuthFullValue,
            CorrelationId = currentCorrelationId,
        };

        var response = await bulkRedactionSearchService.InitiateOrOrchestrateOcr(bulkRedactionSearchDto, orchestrationClient, cancellationToken);

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
