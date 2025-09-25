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

public class GetWitnessStatements : BaseFunction
{
    private readonly ILogger<GetWitnessStatements> _logger;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IMdsClient _mdsClient;

    public GetWitnessStatements(
        ILogger<GetWitnessStatements> logger,
        IDdeiArgFactory ddeiArgFactory,
        IMdsClient mdsClient)
    {
        _logger = logger.ExceptionIfNull();
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
    }

    [Function(nameof(GetWitnessStatements))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.WitnessStatements)] HttpRequest req, string caseUrn, int caseId, int witnessId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var witnessStatementsArgDto = _ddeiArgFactory.CreateWitnessStatementsArgDto(cmsAuthValues, correlationId, caseUrn, caseId, witnessId);
        var witnessStatementDtos = await _mdsClient.GetWitnessStatementsAsync(witnessStatementsArgDto);

        return new OkObjectResult(witnessStatementDtos);
    }
}