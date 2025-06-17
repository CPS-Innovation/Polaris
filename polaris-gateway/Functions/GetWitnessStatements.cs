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

public class GetWitnessStatements : BaseFunction
{
    private readonly ILogger<GetWitnessStatements> _logger;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IDdeiClientFactory _ddeiClientFactory;

    public GetWitnessStatements(
        ILogger<GetWitnessStatements> logger,
        IDdeiArgFactory ddeiArgFactory,
        IDdeiClientFactory ddeiClientFactory)
    {
        _logger = logger.ExceptionIfNull();
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
        _ddeiClientFactory = ddeiClientFactory.ExceptionIfNull();
    }

    [Function(nameof(GetWitnessStatements))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.WitnessStatements)] HttpRequest req, string caseUrn, int caseId, int witnessId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var witnessStatementsArgDto = _ddeiArgFactory.CreateWitnessStatementsArgDto(cmsAuthValues, correlationId, caseUrn, caseId, witnessId);
        var ddeiClient = _ddeiClientFactory.Create(cmsAuthValues, DdeiClients.Mds);
        var witnessStatementDtos = await ddeiClient.GetWitnessStatementsAsync(witnessStatementsArgDto);

        return new OkObjectResult(witnessStatementDtos);
    }
}