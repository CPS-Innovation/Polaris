using Common.Configuration;
using Common.Extensions;
using Ddei.Factories;
using DdeiClient.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using DdeiClient.Enums;

namespace PolarisGateway.Functions;

public class CheckoutDocument : BaseFunction
{
    private readonly ILogger<CheckoutDocument> _logger;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IDdeiClientFactory _ddeiClientFactory;

    public CheckoutDocument(
        ILogger<CheckoutDocument> logger,
        IDdeiArgFactory ddeiArgFactory,
        IDdeiClientFactory ddeiClientFactory)
    {
        _logger = logger.ExceptionIfNull();
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
        _ddeiClientFactory = ddeiClientFactory.ExceptionIfNull();
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

        var ddeiClient = _ddeiClientFactory.Create(cmsAuthValues, DdeiClients.Mds);

        var checkoutDocumentDto = await ddeiClient.CheckoutDocumentAsync(ddeiDocumentIdAndVersionIdArgDto);

        return checkoutDocumentDto.IsSuccess ? new OkResult() : new ConflictObjectResult(checkoutDocumentDto.LockingUserName);
    }
}