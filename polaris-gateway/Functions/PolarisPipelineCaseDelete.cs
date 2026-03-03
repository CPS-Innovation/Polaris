using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using PolarisGateway.Clients.Coordinator;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using PolarisGateway.Extensions;
using Common.Telemetry;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using System.Net;
using System.Net.Http;

namespace PolarisGateway.Functions;

public class PolarisPipelineCaseDelete : BaseFunction
{
    private readonly ILogger<PolarisPipelineCaseDelete> _logger;
    private readonly ICoordinatorClient _coordinatorClient;
    private readonly ITelemetryClient _telemetryClient;

    public PolarisPipelineCaseDelete(
        ILogger<PolarisPipelineCaseDelete> logger,
        ICoordinatorClient coordinatorClient,
        ITelemetryClient telemetryClient)
        : base()
    {
        _logger = logger;
        _coordinatorClient = coordinatorClient;
        _telemetryClient = telemetryClient;

    }

    [Function(nameof(PolarisPipelineCaseDelete))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [OpenApiOperation(operationId: nameof(PolarisPipelineCaseDelete), tags: ["Case"], Summary = "Polaris Pipeline Case - Delete", Description = "Returns case information using caseURN and caseId")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case to add a new action plan.", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Summary = "Case found", Description = "Returns case details")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]


    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = RestApi.Case)] HttpRequest req, string caseUrn, int caseId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);


        return await (await _coordinatorClient.DeleteCaseAsync(
            caseUrn,
            caseId,
            cmsAuthValues,
            correlationId))
            .ToActionResult();
    }
}