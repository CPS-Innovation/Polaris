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

public class GetExhibitProducers : BaseFunction
{
    private readonly ILogger<GetExhibitProducers> _logger;
    private readonly IMdsClient _mdsClient;
    private readonly IMdsArgFactory _mdsArgFactory;

    public GetExhibitProducers(ILogger<GetExhibitProducers> logger,
        IMdsClient mdsClient,
        IMdsArgFactory mdsArgFactory)
        : base()
    {
        _logger = logger.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
        _mdsArgFactory = mdsArgFactory.ExceptionIfNull();
    }

    [Function(nameof(GetExhibitProducers))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseExhibitProducers)] HttpRequest req, string caseUrn, int caseId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var mdsCaseIdentifiersArgDto = _mdsArgFactory.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, caseUrn, caseId);
        
        var exhibitProducerDtos = await _mdsClient.GetExhibitProducersAsync(mdsCaseIdentifiersArgDto);

        return new OkObjectResult(exhibitProducerDtos);
    }
}