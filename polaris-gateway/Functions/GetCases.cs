using Common.Configuration;
using Common.Dto.Response.Case;
using Common.Extensions;
using Ddei.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PolarisGateway.Services.DdeiOrchestration;
using System.Collections.Generic;
using System.Net;
using PolarisGateway.Services.MdsOrchestration;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class GetCases : BaseFunction
{
    private readonly ILogger<GetCases> _logger;
    private readonly IMdsArgFactory _mdsArgFactory;
    private readonly IMdsCaseOrchestrationService _mdsOrchestrationService;

    public GetCases(
        ILogger<GetCases> logger,
        IMdsArgFactory mdsArgFactory,
        IMdsCaseOrchestrationService mdsOrchestrationService)
        : base()
    {
        _logger = logger.ExceptionIfNull();
        _mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        _mdsOrchestrationService = mdsOrchestrationService.ExceptionIfNull();
    }

    [Function(nameof(GetCases))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(GetCases), tags: ["Cases"], Summary = "Get Case List", Description = "Returns cases list information using caseURN")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<CaseDto>), Summary = "List of cases for a case URN", Description = "Returns case list for the given URN")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]

    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Cases)] HttpRequest req, string caseUrn)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var arg = _mdsArgFactory.CreateUrnArg(cmsAuthValues, correlationId, caseUrn);

        var result = await _mdsOrchestrationService.GetCases(arg);

        return new OkObjectResult(result);
    }
}