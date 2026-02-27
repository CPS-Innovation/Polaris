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

public class GetCases : BaseFunction
{
    private readonly ILogger<GetCases> _logger;
    private readonly IMdsArgFactory _mdsArgFactory;
    private readonly IMdsCaseOrchestrationService _mdsOrchestrationService;

    public GetCases(
        ILogger<GetCases> logger,
        IMdsArgFactory mdsArgFactory,
        IMdsCaseOrchestrationService mdsOrchestrationService)
        : base()
    {
        _logger = logger.ExceptionIfNull();
        _mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        _mdsOrchestrationService = mdsOrchestrationService.ExceptionIfNull();
    }

    [Function(nameof(GetCases))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Cases)] HttpRequest req, string caseUrn)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var arg = _mdsArgFactory.CreateUrnArg(cmsAuthValues, correlationId, caseUrn);

        var result = await _mdsOrchestrationService.GetCases(arg);

        return new OkObjectResult(result);
    }
}