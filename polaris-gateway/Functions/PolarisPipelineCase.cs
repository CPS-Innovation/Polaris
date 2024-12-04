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

// note: the analytics KQL queries refer to "PolarisPipelineCase" as the function name,
//  if we change this then we must change the KQL queries to be `| ... ("PolarisPipelineCase" or "NewName")
public class PolarisPipelineCase : BaseFunction
{
    private readonly ILogger<PolarisPipelineCase> _logger;
    private readonly ICoordinatorClient _coordinatorClient;
    private readonly ITelemetryClient _telemetryClient;

    public PolarisPipelineCase(
        ILogger<PolarisPipelineCase> logger,
        ICoordinatorClient coordinatorClient,
        ITelemetryClient telemetryClient)
        : base(telemetryClient)
    {
        _logger = logger;
        _coordinatorClient = coordinatorClient;
        _telemetryClient = telemetryClient;
    }

    [Function(nameof(PolarisPipelineCase))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.Case)] HttpRequest req, string caseUrn, int caseId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        return await (await _coordinatorClient.RefreshCaseAsync(caseUrn, caseId, cmsAuthValues, correlationId)).ToActionResult();
    }
}

