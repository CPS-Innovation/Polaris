// <copyright file="GetCaseSearchIndexCount.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Functions.Maintenance;

using System.Threading.Tasks;
using Common.Configuration;
using Common.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using coordinator.Clients.TextExtractor;
using Microsoft.Azure.Functions.Worker;
using DdeiClient.Services.CaseUrnResolver;
using Common.Dto.Request;
using System.Threading;

public class GetCaseSearchIndexCount(ITextExtractorClient textExtractorClient, ICaseUrnResolver caseUrnResolver)
{
    [Function(nameof(GetCaseSearchIndexCount))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HttpStart([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseSearchCount)] HttpRequest req, int caseId, CancellationToken cancellationToken)
    {
        var currentCorrelationId = req.Headers.GetCorrelationId();

        var searchIndexCount = await textExtractorClient.GetCaseIndexCount(null, caseId, currentCorrelationId, isLegacy: false);

        return new OkObjectResult(searchIndexCount);
    }
}
