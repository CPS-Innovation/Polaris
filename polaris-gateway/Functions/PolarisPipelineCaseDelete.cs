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

public class PolarisPipelineCaseDelete : BaseFunction
{
    private readonly ILogger<PolarisPipelineCaseDelete> _logger;
    private readonly ICoordinatorClient _coordinatorClient;
    private readonly ITelemetryClient _telemetryClient;

    public PolarisPipelineCaseDelete(
        ILogger<PolarisPipelineCaseDelete> logger,
        ICoordinatorClient coordinatorClient,
        ITelemetryClient telemetryClient)
        : base(telemetryClient)
    {
        _logger = logger;
        _coordinatorClient = coordinatorClient;
        _telemetryClient = telemetryClient;

    }

    [Function(nameof(PolarisPipelineCaseDelete))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
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

