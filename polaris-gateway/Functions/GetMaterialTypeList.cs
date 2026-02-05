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

public class GetMaterialTypeList : BaseFunction
{
    private readonly ILogger<GetMaterialTypeList> _logger;
    private readonly IMdsArgFactory _mdsArgFactory;
    private readonly IMdsClient _mdsClient;

    public GetMaterialTypeList(
        ILogger<GetMaterialTypeList> logger,
        IMdsArgFactory mdsArgFactory,
        IMdsClient mdsClient)
    {
        _logger = logger.ExceptionIfNull();
        _mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
    }

    [Function(nameof(GetMaterialTypeList))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.MaterialTypeList)] HttpRequest req)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var mdsBaseArgDto = _mdsArgFactory.CreateCmsCaseDataArgDto(cmsAuthValues, correlationId);
        var materialTypes = await _mdsClient.GetMaterialTypeListAsync(mdsBaseArgDto);

        return new OkObjectResult(materialTypes);
    }
}