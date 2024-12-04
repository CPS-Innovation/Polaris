using Common.Configuration;
using Common.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Extensions;
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
        : base(telemetryClient)
    {
        _logger = logger;
        _coordinatorClient = coordinatorClient;
        _telemetryClient = telemetryClient;
    }

    [Function(nameof(PolarisPipelineCaseSearchIndexCount))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

