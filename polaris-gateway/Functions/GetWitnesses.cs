using Common.Configuration;
using Common.Dto.Response.Case;
using Common.Extensions;
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
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class GetWitnesses : BaseFunction
{
    private readonly ILogger<GetWitnesses> _logger;
    private readonly IMdsArgFactory _mdsArgFactory;
    private readonly IMdsClient _mdsClient;
    private readonly ICaseWitnessMapper _caseWitnessMapper;
    public GetWitnesses(
        ILogger<GetWitnesses> logger,
        IMdsArgFactory mdsArgFactory,
        IMdsClient mdsClient,
        ICaseWitnessMapper caseWitnessMapper)
        : base()
    {
        _logger = logger.ExceptionIfNull();
        _mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
        _caseWitnessMapper = caseWitnessMapper.ExceptionIfNull();
    }

    [Function(nameof(GetWitnesses))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [OpenApiOperation(operationId: nameof(GetWitnesses), tags: ["Case"], Summary = "Get Witnesses", Description = "Returns witnesses information using caseURN and caseId")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case to add a new action plan.", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(IEnumerable<CaseWitnessDto>), Summary = "Case found", Description = "Returns case details")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]

    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseWitnesses)] HttpRequest req, string caseUrn, int caseId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var arg = _mdsArgFactory.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, caseUrn, caseId);

        var caseWitnessResponses = await _mdsClient.GetWitnessesAsync(arg);
        var caseWitnesses = caseWitnessResponses.Select(_caseWitnessMapper.Map);

        return new OkObjectResult(caseWitnesses);
    }
}