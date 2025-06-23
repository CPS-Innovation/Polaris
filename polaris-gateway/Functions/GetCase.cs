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

public class GetCase : BaseFunction
{
    private readonly ILogger<GetCase> _logger;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IDdeiCaseOrchestrationService _ddeiOrchestrationService;

    public GetCase(
        ILogger<GetCase> logger,
        IDdeiArgFactory ddeiArgFactory,
        IDdeiCaseOrchestrationService ddeiOrchestrationService)
        : base()
    {
        _logger = logger.ExceptionIfNull();
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
        _ddeiOrchestrationService = ddeiOrchestrationService.ExceptionIfNull();
    }

    [Function(nameof(GetCase))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Case)] HttpRequest req, string caseUrn, int caseId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var arg = _ddeiArgFactory.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, caseUrn, caseId);

        var result = await _ddeiOrchestrationService.GetCase(arg);

        return new OkObjectResult(result);
    }
}