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

public class GetMaterialTypeList : BaseFunction
{
    private readonly ILogger<GetMaterialTypeList> _logger;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IDdeiClientFactory _ddeiClientFactory;

    public GetMaterialTypeList(
        ILogger<GetMaterialTypeList> logger,
        IDdeiArgFactory ddeiArgFactory, 
        IDdeiClientFactory ddeiClientFactory)
    {
        _logger = logger.ExceptionIfNull();
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
        _ddeiClientFactory = ddeiClientFactory.ExceptionIfNull();
    }

    [Function(nameof(GetMaterialTypeList))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.MaterialTypeList)] HttpRequest req)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var ddeiBaseArgDto = _ddeiArgFactory.CreateCmsCaseDataArgDto(cmsAuthValues, correlationId);
        var ddeiClient = _ddeiClientFactory.Create(cmsAuthValues, DdeiClients.Mds);
        var materialTypes = await ddeiClient.GetMaterialTypeListAsync(ddeiBaseArgDto);

        return new OkObjectResult(materialTypes);
    }
}