using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using PolarisGateway.Clients.Coordinator;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using PolarisGateway.Extensions;
using Common.Telemetry;

namespace PolarisGateway.Functions;

public class PolarisPipelineGetCaseTracker : BaseFunction
{
    private readonly ILogger<PolarisPipelineGetCaseTracker> _logger;
    private readonly ICoordinatorClient _coordinatorClient;
    private readonly ITelemetryClient _telemetryClient;

    public PolarisPipelineGetCaseTracker(
        ILogger<PolarisPipelineGetCaseTracker> logger,
        ICoordinatorClient coordinatorClient,
        ITelemetryClient telemetryClient)
        : base()
    {
        _logger = logger;
        _coordinatorClient = coordinatorClient;
        _telemetryClient = telemetryClient;
    }

    [Function(nameof(PolarisPipelineGetCaseTracker))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseTracker)] HttpRequest req, string caseUrn, int caseId)
    {
        var correlationId = EstablishCorrelation(req);

        return await (await _coordinatorClient.GetTrackerGetCaseAsync(
                caseUrn,
                caseId,
                correlationId))
            .ToActionResult();
    }
}