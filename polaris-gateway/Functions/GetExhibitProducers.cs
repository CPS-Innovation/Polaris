// <copyright file="GetExhibitProducers.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Configuration;
using Common.Dto.Request;
using Common.Dto.Response.Case;
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
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

public class GetExhibitProducers : BaseFunction
{
    private readonly IMdsClient mdsClient;
    private readonly IMdsArgFactory mdsArgFactory;
    private readonly ICaseUrnResolver caseUrnResolver;

    public GetExhibitProducers(
        IMdsClient mdsClient,
        IMdsArgFactory mdsArgFactory,
        ICaseUrnResolver caseUrnResolver)
        : base()
    {
        this.mdsClient = mdsClient.ExceptionIfNull();
        this.mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        this.caseUrnResolver = caseUrnResolver.ExceptionIfNull();
    }

    [Function(nameof(GetExhibitProducers))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(GetExhibitProducers), tags: ["Case"], Summary = "Get Exhibit Producers", Description = "Returns exhibit producers using caseURN and caseId")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case to add a new action plan.", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<ExhibitProducerDto>), Summary = "Case exhibit producers", Description = "Returns the list of exhibit producers")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseExhibitProducers)] HttpRequest req, int caseId, CancellationToken cancellationToken = default)
    {
        var correlationId = EstablishCorrelation(req);
        CmsAuthValues cmsAuthValues = req.BuildCmsAuthValues();

        var caseUrn = await this.caseUrnResolver.ResolveCaseUrnAsync(caseId, cmsAuthValues, cancellationToken);

        var mdsCaseIdentifiersArgDto = this.mdsArgFactory.CreateCaseIdentifiersArg(cmsAuthValues.CmsAuthFullValue, correlationId, caseUrn, caseId);

        var exhibitProducerDtos = await this.mdsClient.GetExhibitProducersAsync(mdsCaseIdentifiersArgDto, cancellationToken: cancellationToken);

        return new OkObjectResult(exhibitProducerDtos);
    }
}
