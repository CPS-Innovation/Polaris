using Common.Configuration;
using Common.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Extensions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class PolarisPipelineCaseSearchIndexCount : BaseFunction
{
    private readonly ILogger<PolarisPipelineCaseSearchIndexCount> _logger;
    private readonly ICoordinatorClient _coordinatorClient;
    private readonly ITelemetryClient _telemetryClient;

    public PolarisPipelineCaseSearchIndexCount(
        ILogger<PolarisPipelineCaseSearchIndexCount> logger,
        ICoordinatorClient coordinatorClient,
        ITelemetryClient telemetryClient)
        : base()
    {
        _logger = logger;
        _coordinatorClient = coordinatorClient;
        _telemetryClient = telemetryClient;
    }

    [Function(nameof(PolarisPipelineCaseSearchIndexCount))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation(operationId: nameof(PolarisPipelineCaseSearchIndexCount), tags: ["Case"], Summary = "Polaris Pipeline Case - Search Index Count", Description = "Returns case information using caseURN and caseId")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case to add a new action plan.", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Summary = "Case found", Description = "Returns case details")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]


    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseSearchCount)] HttpRequest req, string caseUrn, int caseId)
    {
        var correlationId = EstablishCorrelation(req);

        return await (await _coordinatorClient.GetCaseSearchIndexCount(
                caseUrn,
                caseId,
                correlationId))
                .ToActionResult();
    }
}