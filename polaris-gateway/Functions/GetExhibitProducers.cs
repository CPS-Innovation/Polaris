using Common.Configuration;
using Common.Dto.Response.Case;
using Common.Extensions;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
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
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class GetExhibitProducers : BaseFunction
{
    private readonly ILogger<GetExhibitProducers> _logger;
    private readonly IMdsClient _mdsClient;
    private readonly IMdsArgFactory _mdsArgFactory;

    public GetExhibitProducers(ILogger<GetExhibitProducers> logger,
        IMdsClient mdsClient,
        IMdsArgFactory mdsArgFactory)
        : base()
    {
        _logger = logger.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
        _mdsArgFactory = mdsArgFactory.ExceptionIfNull();
    }

    [Function(nameof(GetExhibitProducers))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(GetExhibitProducers), tags: ["Case"], Summary = "Get Exhibit Producers", Description = "Returns exhibit producers using caseURN and caseId")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case to add a new action plan.", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<ExhibitProducerDto>), Summary = "Case exhibit producers", Description = "Returns the list of exhibit producers")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseExhibitProducers)] HttpRequest req, string caseUrn, int caseId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var mdsCaseIdentifiersArgDto = _mdsArgFactory.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, caseUrn, caseId);
        
        var exhibitProducerDtos = await _mdsClient.GetExhibitProducersAsync(mdsCaseIdentifiersArgDto);

        return new OkObjectResult(exhibitProducerDtos);
    }
}