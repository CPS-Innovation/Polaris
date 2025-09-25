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

public class GetMaterialTypeList : BaseFunction
{
    private readonly ILogger<GetMaterialTypeList> _logger;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IMdsClient _mdsClient;

    public GetMaterialTypeList(
        ILogger<GetMaterialTypeList> logger,
        IDdeiArgFactory ddeiArgFactory, 
        IMdsClient mdsClient)
    {
        _logger = logger.ExceptionIfNull();
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
    }

    [Function(nameof(GetMaterialTypeList))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.MaterialTypeList)] HttpRequest req)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var ddeiBaseArgDto = _ddeiArgFactory.CreateCmsCaseDataArgDto(cmsAuthValues, correlationId);
        var materialTypes = await _mdsClient.GetMaterialTypeListAsync(ddeiBaseArgDto);

        return new OkObjectResult(materialTypes);
    }
}