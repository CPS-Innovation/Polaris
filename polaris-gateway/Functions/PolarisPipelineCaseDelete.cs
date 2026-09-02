// <copyright file="PolarisPipelineCaseDelete.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using PolarisGateway.Clients.Coordinator;
using Microsoft.Azure.Functions.Worker;
using System.Threading;
using System.Threading.Tasks;
using PolarisGateway.Extensions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using System.Net;
using System.Net.Http;
using DdeiClient.Services.CaseUrnResolver;
using Common.Dto.Request;
using Common.Extensions;

public class PolarisPipelineCaseDelete(ICoordinatorClient coordinatorClient) : BaseFunction
{
    [Function(nameof(PolarisPipelineCaseDelete))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [OpenApiOperation(operationId: nameof(PolarisPipelineCaseDelete), tags: ["Case"], Summary = "Polaris Pipeline Case - Delete", Description = "Returns case information using caseURN and caseId")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case to add a new action plan.", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Summary = "Case found", Description = "Returns case details")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]

    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = RestApi.Case)] HttpRequest req, int caseId, CancellationToken cancellationToken = default)
    {
        var correlationId = EstablishCorrelation(req);
        CmsAuthValues cmsAuthValues = req.BuildCmsAuthValues();

        cancellationToken.ThrowIfCancellationRequested();

        return await (await coordinatorClient.DeleteCaseAsync(
                null,
                caseId,
                cmsAuthValues.CmsAuthFullValue,
                correlationId,
                isLegacy: false))
            .ToActionResult();
    }
}
