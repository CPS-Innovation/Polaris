// <copyright file="PolarisPipelineBulkRedactionSearchResults.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Extensions;
using PolarisGateway.Services.Artefact.Domain;

public class PolarisPipelineBulkRedactionSearchResults(ICoordinatorClient coordinatorClient) : BaseFunction
{
    private const string SearchTextHeader = "SearchText";

    [Function(nameof(PolarisPipelineBulkRedactionSearchResults))]
    [OpenApiOperation(operationId: nameof(PolarisPipelineBulkRedactionSearchResults), tags: ["Documents"], Summary = "OCR Search Result", Description = "Gives the OCR search results")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the material", Required = true)]
    [OpenApiParameter("documentId", In = ParameterLocation.Path, Type = typeof(long), Description = "The document Id (version) of the material", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Summary = "OCR Search Result", Description = "Gives the OCR search results")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]
    public async Task<IActionResult> RunPost([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.OcrSearch)]HttpRequest req, int caseId, string materialId, long documentId, CancellationToken cancellationToken)
    {
        var searchText = req.Query[SearchTextHeader];
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        return await (
            await coordinatorClient.BulkRedactionRetrieveSearchResultsAsync(
                caseId,
                materialId,
                documentId,
                searchText,
                correlationId,
                cmsAuthValues,
                cancellationToken))
            .ToActionResult();
    }
}
