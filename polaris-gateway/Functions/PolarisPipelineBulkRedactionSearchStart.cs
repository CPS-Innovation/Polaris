// <copyright file="PolarisPipelineBulkRedactionSearchStart.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Extensions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class PolarisPipelineBulkRedactionSearchStart(ICoordinatorClient coordinatorClient) : BaseFunction
{
    [Function(nameof(PolarisPipelineBulkRedactionSearchStart))]
    [OpenApiOperation(operationId: nameof(PolarisPipelineBulkRedactionSearchStart), tags: ["Documents"], Summary = "Orchestration Status", Description = "Schedule new orchestration instance or returns status")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiParameter("materialId", In = ParameterLocation.Path, Type = typeof(string), Description = "The Id of the material", Required = true)]
    [OpenApiParameter("documentId", In = ParameterLocation.Path, Type = typeof(long), Description = "The document Id (version) of the material", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Summary = "Orchestration Status", Description = "Schedule new orchestration instance or returns status")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]

    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.OcrSearch)] HttpRequest req, int caseId, string materialId, long documentId, CancellationToken cancellationToken)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        return await (await coordinatorClient.BulkRedactionInitiateSearchAsync(caseId, materialId, documentId, correlationId, cmsAuthValues, cancellationToken)).ToActionResult();
    }
}
