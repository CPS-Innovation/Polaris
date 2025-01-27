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
using DdeiClient.Domain.Args;

namespace PolarisGateway.Functions;

public class ToggleIsUnusedDocument : BaseFunction
{
    private readonly ILogger<ToggleIsUnusedDocument> _logger;
    private readonly IDdeiClient _ddeiClient;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly ITelemetryClient _telemetryClient;

    public ToggleIsUnusedDocument(
        ILogger<ToggleIsUnusedDocument> logger,
        IDdeiClient ddeiClient,
        IDdeiArgFactory ddeiArgFactory,
        ITelemetryClient telemetryClient)
        : base()
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
        _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
        _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
    }

    [Function(nameof(ToggleIsUnusedDocument))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ToggleIsUnusedDocument)] HttpRequest req, string caseUrn, int caseId, long documentId, string isUnused)
    {
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        var toggleIsUnusedDocumentDto = new DdeiToggleIsUnusedDocumentDto
        {
            CaseId = caseId,
            CmsAuthValues = cmsAuthValues,
            CorrelationId = correlationId,
            DocumentId = documentId,
            IsUnused = isUnused,
            Urn = caseUrn,
        };

        return await _ddeiClient.ToggleIsUnusedDocumentAsync(toggleIsUnusedDocumentDto) ? new OkResult() : new BadRequestResult();
    }
}