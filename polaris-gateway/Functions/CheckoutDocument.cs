using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using Ddei;
using Ddei.Factories;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using System;
using Common.Telemetry;

namespace PolarisGateway.Functions;

public class CheckoutDocument : BaseFunction
{
    private readonly ILogger<CheckoutDocument> _logger;
    private readonly IDdeiClient _ddeiClient;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly ITelemetryClient _telemetryClient;

    public CheckoutDocument(
        ILogger<CheckoutDocument> logger,
        IDdeiClient ddeiClient,
        IDdeiArgFactory ddeiArgFactory,
        ITelemetryClient telemetryClient)
        : base(telemetryClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
        _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
        _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
    }

    [Function(nameof(CheckoutDocument))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.DocumentCheckout)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var arg = _ddeiArgFactory.CreateDocumentVersionArgDto(
                     cmsAuthValues: cmsAuthValues,
                     correlationId: correlationId,
                     urn: caseUrn,
                     caseId: caseId,
                     documentId: documentId,
                     versionId: versionId);

        var result = await _ddeiClient.CheckoutDocumentAsync(arg);

        return result.IsSuccess ? new OkResult() : new ConflictObjectResult(result.LockingUserName);
    }
}