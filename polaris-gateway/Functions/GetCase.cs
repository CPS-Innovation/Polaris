using Common.Configuration;
using Common.Extensions;
using Ddei.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PolarisGateway.Services.MdsOrchestration;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class GetCase : BaseFunction
{
    private readonly ILogger<GetCase> _logger;
    private readonly IMdsArgFactory _mdsArgFactory;
    private readonly IMdsCaseOrchestrationService _mdsOrchestrationService;

    public GetCase(
        ILogger<GetCase> logger,
        IMdsArgFactory mdsArgFactory,
        IMdsCaseOrchestrationService mdsOrchestrationService)
        : base()
    {
        _logger = logger.ExceptionIfNull();
        _mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        _mdsOrchestrationService = mdsOrchestrationService.ExceptionIfNull();
    }

    [Function(nameof(GetCase))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Case)] HttpRequest req, string caseUrn, int caseId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var arg = _mdsArgFactory.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, caseUrn, caseId);

        var result = await _mdsOrchestrationService.GetCase(arg);

        return new OkObjectResult(result);
    }
}