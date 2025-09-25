using Common.Configuration;
using Common.Extensions;
using DdeiClient.Clients.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using DdeiClient.Factories;

namespace PolarisGateway.Functions;

public class GetExhibitProducers : BaseFunction
{
    private readonly ILogger<GetExhibitProducers> _logger;
    private readonly IMdsClient _mdsClient;
    private readonly IDdeiArgFactory _ddeiArgFactory;

    public GetExhibitProducers(ILogger<GetExhibitProducers> logger,
        IMdsClient mdsClient,
        IDdeiArgFactory ddeiArgFactory)
        : base()
    {
        _logger = logger.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
    }

    [Function(nameof(GetExhibitProducers))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseExhibitProducers)] HttpRequest req, string caseUrn, int caseId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var ddeiCaseIdentifiersArgDto = _ddeiArgFactory.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, caseUrn, caseId);
        
        var exhibitProducerDtos = await _mdsClient.GetExhibitProducersAsync(ddeiCaseIdentifiersArgDto);

        return new OkObjectResult(exhibitProducerDtos);
    }
}