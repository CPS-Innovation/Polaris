using Common.Configuration;
using Common.Extensions;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class LookupUrn : BaseFunction
{
    private readonly ILogger<LookupUrn> _logger;
    private readonly IMdsClient _mdsClient;
    private readonly IMdsArgFactory _mdsArgFactory;

    public LookupUrn(
        ILogger<LookupUrn> logger,
        IMdsClient mdsClient,
        IMdsArgFactory mdsArgFactory)
        : base()
    {
        _logger = logger.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
        _mdsArgFactory = mdsArgFactory.ExceptionIfNull();
    }

    [Function(nameof(LookupUrn))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.LookupUrn)] HttpRequest req, int caseId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var arg = _mdsArgFactory.CreateCaseIdArg(cmsAuthValues, correlationId, caseId);

        var result = await _mdsClient.GetUrnFromCaseIdAsync(arg);

        return new OkObjectResult(result);
    }
}