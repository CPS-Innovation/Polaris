using Common.Configuration;
using Common.Extensions;
using Ddei.Factories;
using DdeiClient.Enums;
using DdeiClient.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class GetExhibitProducers : BaseFunction
{
    private readonly ILogger<GetExhibitProducers> _logger;
    private readonly IDdeiClientFactory _ddeiClientFactory;
    private readonly IDdeiArgFactory _ddeiArgFactory;

    public GetExhibitProducers(ILogger<GetExhibitProducers> logger,
        IDdeiClientFactory ddeiClientFactory,
        IDdeiArgFactory ddeiArgFactory)
        : base()
    {
        _logger = logger.ExceptionIfNull();
        _ddeiClientFactory = ddeiClientFactory.ExceptionIfNull();
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
    }

    [Function(nameof(GetExhibitProducers))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseExhibitProducers)] HttpRequest req, string caseUrn, int caseId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var ddeiCaseIdentifiersArgDto = _ddeiArgFactory.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, caseUrn, caseId);
        var ddeiClient = _ddeiClientFactory.Create(cmsAuthValues, DdeiClients.Mds);
        var exhibitProducerDtos = await ddeiClient.GetExhibitProducersAsync(ddeiCaseIdentifiersArgDto);

        return new OkObjectResult(exhibitProducerDtos);
    }
}