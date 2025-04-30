using Common.Configuration;
using Common.Telemetry;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class GetWitnesses : BaseFunction
{
    private readonly ILogger<GetWitnesses> _logger;
    private readonly IDdeiClient _ddeiClient;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly ITelemetryClient _telemetryClient;

    public GetWitnesses(
        ILogger<GetWitnesses> logger,
        [FromKeyedServices(DdeiClients.Ddei)] IDdeiClient ddeiClient,
        IDdeiArgFactory ddeiArgFactory,
        ITelemetryClient telemetryClient)
        : base()
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
        _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
        _telemetryClient = telemetryClient;
    }

    [Function(nameof(GetWitnesses))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseWitnesses)] HttpRequest req, string caseUrn, int caseId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var arg = _ddeiArgFactory.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, caseUrn, caseId);
        var result = await _ddeiClient.GetWitnessesAsync(arg);

        return new OkObjectResult(result);
    }
}