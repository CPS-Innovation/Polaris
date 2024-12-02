using Common.Configuration;
using Common.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Extensions;
using System;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class PolarisPipelineCaseSearch : BaseFunction
{
    private const string Query = "query";
    private readonly ILogger<PolarisPipelineCaseSearch> _logger;
    private readonly ICoordinatorClient _coordinatorClient;
    private readonly ITelemetryClient _telemetryClient;

    public PolarisPipelineCaseSearch(
        ILogger<PolarisPipelineCaseSearch> logger,
        ICoordinatorClient coordinatorClient,
        ITelemetryClient telemetryClient)
        : base(telemetryClient)
    {
        _logger = logger;
        _coordinatorClient = coordinatorClient;
        _telemetryClient = telemetryClient;
    }


    [Function(nameof(PolarisPipelineCaseSearch))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseSearch)] HttpRequest req, string caseUrn, int caseId)
    {
        var correlationId = EstablishCorrelation(req);

        return await (await _coordinatorClient.SearchCase(
                caseUrn,
                caseId,
                req.Query[Query],
                correlationId))
                .ToActionResult();
    }
}

