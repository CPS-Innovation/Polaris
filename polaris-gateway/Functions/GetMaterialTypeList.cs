using Common.Configuration;
using Common.Telemetry;
using Ddei;
using Ddei.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class GetMaterialTypeList : BaseFunction
{
    private readonly ILogger<GetMaterialTypeList> _logger;
    private readonly IDdeiClient _ddeiClient;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly ITelemetryClient _telemetryClient;

    public GetMaterialTypeList(ILogger<GetMaterialTypeList> logger,
        IDdeiClient ddeiClient,
        IDdeiArgFactory ddeiArgFactory,
        ITelemetryClient telemetryClient)
        : base(telemetryClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
        _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
        _telemetryClient = telemetryClient;
    }

    [Function(nameof(GetMaterialTypeList))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.MaterialTypeList)] HttpRequest req)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var arg = _ddeiArgFactory.CreateCmsCaseDataArgDto(cmsAuthValues, correlationId);
        var result = await _ddeiClient.GetMaterialTypeListAsync(arg);

        return new OkObjectResult(result);
    }
}