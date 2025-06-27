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

public class CancelCheckoutDocument : BaseFunction
{
    private readonly ILogger<CancelCheckoutDocument> _logger;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IMdsClient _mdsClient;

    public CancelCheckoutDocument(
        ILogger<CancelCheckoutDocument> logger,
        IDdeiArgFactory ddeiArgFactory,
        IMdsClient mdsClient)
    {
        _logger = logger.ExceptionIfNull();
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
    }

    [Function(nameof(CancelCheckoutDocument))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = RestApi.DocumentCheckout)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId)
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

        await _mdsClient.CancelCheckoutDocumentAsync(ddeiDocumentIdAndVersionIdArgDto);

        return new OkResult();
    }
}