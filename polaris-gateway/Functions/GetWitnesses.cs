using Common.Configuration;
using Common.Extensions;
using Ddei.Factories;
using Ddei.Mappers;
using DdeiClient.Clients.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class GetWitnesses : BaseFunction
{
    private readonly ILogger<GetWitnesses> _logger;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IMdsClient _mdsClient;
    private readonly ICaseWitnessMapper _caseWitnessMapper;
    public GetWitnesses(
        ILogger<GetWitnesses> logger,
        IDdeiArgFactory ddeiArgFactory, 
        IMdsClient mdsClient, 
        ICaseWitnessMapper caseWitnessMapper)
        : base()
    {
        _logger = logger.ExceptionIfNull();
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
        _caseWitnessMapper = caseWitnessMapper.ExceptionIfNull();
    }

    [Function(nameof(GetWitnesses))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseWitnesses)] HttpRequest req, string caseUrn, int caseId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var arg = _ddeiArgFactory.CreateCaseIdentifiersArg(cmsAuthValues, correlationId, caseUrn, caseId);
        
        var caseWitnessResponses = await _mdsClient.GetWitnessesAsync(arg);
        var caseWitnesses = caseWitnessResponses.Select(_caseWitnessMapper.Map);

        return new OkObjectResult(caseWitnesses);
    }
}