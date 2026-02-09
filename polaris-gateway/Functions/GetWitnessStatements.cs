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

public class GetWitnessStatements : BaseFunction
{
    private readonly ILogger<GetWitnessStatements> _logger;
    private readonly IMdsArgFactory _mdsArgFactory;
    private readonly IMdsClient _mdsClient;

    public GetWitnessStatements(
        ILogger<GetWitnessStatements> logger,
        IMdsArgFactory mdsArgFactory,
        IMdsClient mdsClient)
    {
        _logger = logger.ExceptionIfNull();
        _mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
    }

    [Function(nameof(GetWitnessStatements))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.WitnessStatements)] HttpRequest req, string caseUrn, int caseId, int witnessId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var witnessStatementsArgDto = _mdsArgFactory.CreateWitnessStatementsArgDto(cmsAuthValues, correlationId, caseUrn, caseId, witnessId);
        var witnessStatementDtos = await _mdsClient.GetWitnessStatementsAsync(witnessStatementsArgDto);

        return new OkObjectResult(witnessStatementDtos);
    }
}