using Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ddei.Factories;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using System;
using Common.Telemetry;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace PolarisGateway.Functions;

public class CancelCheckoutDocument : BaseFunction
{
    private readonly ILogger<CancelCheckoutDocument> _logger;
    private readonly IDdeiClient _ddeiClient;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly ITelemetryClient _telemetryClient;

    public CancelCheckoutDocument(
        ILogger<CancelCheckoutDocument> logger,
        [FromKeyedServices(DdeiClients.Ddei)] IDdeiClient ddeiClient,
        IDdeiArgFactory ddeiArgFactory,
        ITelemetryClient telemetryClient)
        : base()
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
        _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
        _telemetryClient = telemetryClient;
    }

    [Function(nameof(CancelCheckoutDocument))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = RestApi.DocumentCheckout)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId)
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

        await _ddeiClient.CancelCheckoutDocumentAsync(arg);

        return new OkResult();
    }
}