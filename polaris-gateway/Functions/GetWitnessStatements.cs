// <copyright file="GetWitnessStatements.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Configuration;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Extensions;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Services.CaseUrnResolver;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class GetWitnessStatements : BaseFunction
{
    private readonly IMdsArgFactory mdsArgFactory;
    private readonly IMdsClient mdsClient;
    private readonly ICaseUrnResolver caseUrnResolver;

    public GetWitnessStatements(
        IMdsArgFactory mdsArgFactory,
        IMdsClient mdsClient,
        ICaseUrnResolver caseUrnResolver)
    {
        this.mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        this.mdsClient = mdsClient.ExceptionIfNull();
        this.caseUrnResolver = caseUrnResolver.ExceptionIfNull();
    }

    [Function(nameof(GetWitnessStatements))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(GetWitnessStatements), tags: ["Case"], Summary = "Get Witnesses", Description = "Returns witnesses information using caseURN and caseId")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case to add a new action plan.", Required = true)]
    [OpenApiParameter("witnessId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case to add a new action plan.", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<WitnessStatementDto>), Summary = "Case found", Description = "Returns case details")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]

    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.WitnessStatements)] HttpRequest req, int caseId, int witnessId, CancellationToken cancellationToken = default)
    {
        var correlationId = EstablishCorrelation(req);
        CmsAuthValues cmsAuthValues = req.BuildCmsAuthValues();

        var caseUrn = await this.caseUrnResolver.ResolveCaseUrnAsync(caseId, cmsAuthValues, cancellationToken);

        var witnessStatementsArgDto = this.mdsArgFactory.CreateWitnessStatementsArgDto(cmsAuthValues.CmsAuthFullValue, correlationId, caseUrn, caseId, witnessId);
        var witnessStatementDtos = await this.mdsClient.GetWitnessStatementsAsync(witnessStatementsArgDto, cancellationToken: cancellationToken);

        return new OkObjectResult(witnessStatementDtos);
    }
}
