using Common.Configuration;
using Common.Extensions;
using Ddei.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PolarisGateway.Services.DdeiOrchestration;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class GetCases : BaseFunction
{
    private readonly ILogger<GetCases> _logger;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IDdeiCaseOrchestrationService _ddeiOrchestrationService;

    public GetCases(
        ILogger<GetCases> logger,
        IDdeiArgFactory ddeiArgFactory,
        IDdeiCaseOrchestrationService ddeiOrchestrationService)
        : base()
    {
        _logger = logger.ExceptionIfNull();
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
        _ddeiOrchestrationService = ddeiOrchestrationService.ExceptionIfNull();
    }

    [Function(nameof(GetCases))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Cases)] HttpRequest req, string caseUrn)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var arg = _ddeiArgFactory.CreateUrnArg(cmsAuthValues, correlationId, caseUrn);

        var result = await _ddeiOrchestrationService.GetCases(arg);

        return new OkObjectResult(result);
    }
}