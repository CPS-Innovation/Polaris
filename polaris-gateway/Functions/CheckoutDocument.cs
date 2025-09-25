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

public class CheckoutDocument : BaseFunction
{
    private readonly ILogger<CheckoutDocument> _logger;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IMdsClient _mdsClient;

    public CheckoutDocument(
        ILogger<CheckoutDocument> logger,
        IDdeiArgFactory ddeiArgFactory,
        IMdsClient mdsClient)
    {
        _logger = logger.ExceptionIfNull();
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
    }

    [Function(nameof(CheckoutDocument))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.DocumentCheckout)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var ddeiDocumentIdAndVersionIdArgDto = _ddeiArgFactory.CreateDocumentVersionArgDto(
                     cmsAuthValues: cmsAuthValues,
                     correlationId: correlationId,
                     urn: caseUrn,
                     caseId: caseId,
                     documentId: documentId,
                     versionId: versionId);

        var checkoutDocumentDto = await _mdsClient.CheckoutDocumentAsync(ddeiDocumentIdAndVersionIdArgDto);

        return checkoutDocumentDto.IsSuccess ? new OkResult() : new ConflictObjectResult(checkoutDocumentDto.LockingUserName);
    }
}