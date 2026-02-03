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

public class CheckoutDocument : BaseFunction
{
    private readonly ILogger<CheckoutDocument> _logger;
    private readonly IMdsArgFactory _mdsArgFactory;
    private readonly IMdsClient _mdsClient;

    public CheckoutDocument(
        ILogger<CheckoutDocument> logger,
        IMdsArgFactory mdsArgFactory,
        IMdsClient mdsClient)
    {
        _logger = logger.ExceptionIfNull();
        _mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
    }

    [Function(nameof(CheckoutDocument))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.DocumentCheckout)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var ddeiDocumentIdAndVersionIdArgDto = _mdsArgFactory.CreateDocumentVersionArgDto(
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