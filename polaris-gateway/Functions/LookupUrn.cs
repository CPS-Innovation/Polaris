// <copyright file="LookupUrn.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Configuration;
using Common.Extensions;
using Common.Dto.Request;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class LookupUrn : BaseFunction
{
    private readonly IMasterDataServiceClient masterDataServiceClient;
    private readonly IMdsArgFactory mdsArgFactory;

    public LookupUrn(
        IMasterDataServiceClient masterDataServiceClient,
        IMdsArgFactory mdsArgFactory)
        : base()
    {
        this.masterDataServiceClient = masterDataServiceClient.ExceptionIfNull();
        this.mdsArgFactory = mdsArgFactory.ExceptionIfNull();
    }

    [Function(nameof(LookupUrn))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(LookupUrn), tags: ["Case"], Summary = "Get Case identifiers", Description = "Returns look up urn information using caseId")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case.", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Summary = "Case urn", Description = "Returns case urn details")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]

    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.LookupUrn)] HttpRequest req, int caseId, CancellationToken cancellationToken = default)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var arg = this.mdsArgFactory.CreateCaseIdArg(cmsAuthValues, correlationId, caseId);

        var result = await this.masterDataServiceClient.LookupUrnAsync(arg, new CmsAuthValues(cmsAuthValues, correlationId), cancellationToken);

        return new OkObjectResult(result);
    }
}
