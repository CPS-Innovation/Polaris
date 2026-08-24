// <copyright file="GetCase.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Configuration;
using Common.Extensions;
using Cps.MasterDataService.Infrastructure.ApiClient;
using Ddei.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Net;
using System.Threading;
using PolarisGateway.Services.MdsOrchestration;
using System.Threading.Tasks;
using DdeiClient.Services.CaseUrnResolver;
using Common.Dto.Request;

public class GetCase : BaseFunction
{
    private readonly IMdsArgFactory mdsArgFactory;
    private readonly IMdsCaseOrchestrationService mdsOrchestrationService;
    private readonly ICaseUrnResolver caseUrnResolver;

    public GetCase(
        IMdsArgFactory mdsArgFactory,
        IMdsCaseOrchestrationService mdsOrchestrationService,
        ICaseUrnResolver caseUrnResolver)
        : base()
    {
        this.mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        this.mdsOrchestrationService = mdsOrchestrationService.ExceptionIfNull();
        this.caseUrnResolver = caseUrnResolver.ExceptionIfNull();
    }

    [Function(nameof(GetCase))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(GetCase), tags: ["Case"], Summary = "Get Case", Description = "Returns case information using caseURN and caseId")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case to add a new action plan.", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK,contentType: "application/json",bodyType: typeof(object),Summary = "Case found",Description = "Returns case details")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request",Description = "Missing or invalid parameters")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Case)] HttpRequest req, int caseId, CancellationToken cancellationToken = default)
    {
        var correlationId = EstablishCorrelation(req);

        CmsAuthValues cmsAuthValues = req.BuildCmsAuthValues();

        var caseUrn = await this.caseUrnResolver.ResolveCaseUrnAsync(caseId, cmsAuthValues, cancellationToken);

        var arg = this.mdsArgFactory.CreateCaseIdentifiersArg(cmsAuthValues.CmsAuthFullValue, correlationId, caseUrn, caseId);

        var result = await this.mdsOrchestrationService.GetCase(arg, cancellationToken);

        return new OkObjectResult(result);
    }
}
