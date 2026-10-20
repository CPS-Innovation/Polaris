// <copyright file="GetWitnessesLegacy.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Functions;

using Common.Configuration;
using Common.Dto.Response.Case;
using Common.Extensions;
using Common.Dto.Request.HouseKeeping;
using Ddei.Factories;
using Ddei.Mappers;
using DdeiClient.Clients.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Common.Dto.Request;

public class GetWitnessesLegacy : BaseFunction
{
    private readonly IMdsArgFactory mdsArgFactory;
    private readonly IMasterDataServiceClient masterDataServiceClient;
    private readonly ICaseWitnessMapper caseWitnessMapper;
    public GetWitnessesLegacy(
        IMdsArgFactory mdsArgFactory,
        IMasterDataServiceClient masterDataServiceClient,
        ICaseWitnessMapper caseWitnessMapper)
        : base()
    {
        this.mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        this.masterDataServiceClient = masterDataServiceClient.ExceptionIfNull();
        this.caseWitnessMapper = caseWitnessMapper.ExceptionIfNull();
    }

    [Function(nameof(GetWitnessesLegacy))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(GetWitnessesLegacy), tags: ["Case"], Summary = "Get Witnesses", Description = "Returns witnesses information using caseURN and caseId")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case to add a new action plan.", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<CaseWitnessDto>), Summary = "Case found", Description = "Returns case details")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]

    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseWitnessesLegacy)] HttpRequest req, string caseUrn, int caseId, CancellationToken cancellationToken = default)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var arg = this.mdsArgFactory.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, caseUrn, caseId);

        var witnessesResponse = await this.masterDataServiceClient.GetCaseWitnessesAsync(
            new GetCaseWitnessesRequest(arg.CaseId, arg.CorrelationId),
            new CmsAuthValues(cmsAuthValues, correlationId),
            cancellationToken);

        var witnesses = witnessesResponse.Witnesses.Select(this.caseWitnessMapper.Map);

        return new OkObjectResult(witnesses);
    }
}
