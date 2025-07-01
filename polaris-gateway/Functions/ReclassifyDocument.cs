using Common.Configuration;
using Common.Dto.Request;
using Common.Telemetry;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PolarisGateway.Mappers;
using PolarisGateway.TelemetryEvents;
using PolarisGateway.Validators;
using System;
using System.Net;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class ReclassifyDocument : BaseFunction
{
    private readonly ILogger<ReclassifyDocument> _logger;
    private readonly IDdeiAuthClient _ddeiAuthClient;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IReclassifyDocumentRequestMapper _reclassifyDocumentRequestMapper;
    private readonly ITelemetryClient _telemetryClient;

    public ReclassifyDocument(
        ILogger<ReclassifyDocument> logger,
        IDdeiAuthClient ddeiAuthClient,
        IDdeiArgFactory ddeiArgFactory,
        IReclassifyDocumentRequestMapper reclassifyDocumentRequestMapper,
        ITelemetryClient telemetryClient)
        : base()
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ddeiAuthClient = ddeiAuthClient ?? throw new ArgumentNullException(nameof(ddeiAuthClient));
        _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
        _reclassifyDocumentRequestMapper = reclassifyDocumentRequestMapper ?? throw new ArgumentNullException(nameof(reclassifyDocumentRequestMapper));
        _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
    }

    [Function(nameof(ReclassifyDocument))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ReclassifyDocument)] HttpRequest req, string caseUrn, int caseId, string documentId)
    {
        var telemetryEvent = new DocumentReclassifiedEvent(caseId, documentId)
        {
            OperationName = nameof(ReclassifyDocument),
        };

        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        try
        {
            telemetryEvent.IsRequestValid = true;
            telemetryEvent.CorrelationId = correlationId;

            var body = await ValidatorHelper.GetJsonBody<ReclassifyDocumentDto, ReclassifyDocumentValidator>(req);
            telemetryEvent.IsRequestJsonValid = body.IsValid;
            telemetryEvent.RequestJson = body.RequestJson;

            if (!body.IsValid)
            {
                _telemetryClient.TrackEvent(telemetryEvent);
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            var arg = _ddeiArgFactory.CreateReclassifyDocumentArgDto
            (
                cmsAuthValues: cmsAuthValues,
                correlationId: correlationId,
                urn: caseUrn,
                caseId: caseId,
                documentId: documentId,
                dto: body.Value
            );

            var result = await _ddeiAuthClient.ReclassifyDocumentAsync(arg);

            telemetryEvent.IsSuccess = true;
            _telemetryClient.TrackEvent(telemetryEvent);

            return new ObjectResult(result);
        }
        catch
        {
            _telemetryClient.TrackEventFailure(telemetryEvent);
            throw;
        }
    }
}