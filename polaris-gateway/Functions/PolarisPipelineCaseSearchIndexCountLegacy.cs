using Common.Configuration;
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
using System.Threading;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class PolarisPipelineCaseSearchIndexCountLegacy : BaseFunction
{
    private readonly ILogger<PolarisPipelineCaseSearchIndexCountLegacy> _logger;
    private readonly ICoordinatorClient _coordinatorClient;

    public PolarisPipelineCaseSearchIndexCountLegacy(
        ILogger<PolarisPipelineCaseSearchIndexCountLegacy> logger,
        ICoordinatorClient coordinatorClient)
        : base()
    {
        _logger = logger;
        _coordinatorClient = coordinatorClient;
    }

    [Function(nameof(PolarisPipelineCaseSearchIndexCountLegacy))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [OpenApiOperation(operationId: nameof(PolarisPipelineCaseSearchIndexCountLegacy), tags: ["Case"], Summary = "Polaris Pipeline Case - Search Index Count", Description = "Returns case information using caseURN and caseId")]
    [OpenApiSecurity("Correlation-Id", SecuritySchemeType.ApiKey, Name = "Correlation-Id", In = OpenApiSecurityLocationType.Header, Description = "Must be a valid GUID")]
    [OpenApiParameter(name: "caseUrn", In = ParameterLocation.Query, Required = true, Type = typeof(string), Summary = "Case URN", Description = "The URN identifier of the case")]
    [OpenApiParameter("caseId", In = ParameterLocation.Path, Type = typeof(int), Description = "The Id of the case to add a new action plan.", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(object), Summary = "Case found", Description = "Returns case details")]
    [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.NoContent, Summary = "Invalid request", Description = "Missing or invalid parameters")]


    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseSearchCountLegacy)] HttpRequest req, string caseUrn, int caseId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var correlationId = EstablishCorrelation(req);

        return await (await _coordinatorClient.GetCaseSearchIndexCount(
                caseUrn,
                caseId,
                correlationId))
                .ToActionResult();
    }
}