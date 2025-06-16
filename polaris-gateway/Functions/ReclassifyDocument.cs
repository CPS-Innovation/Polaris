using Common.Configuration;
using Common.Dto.Request;
using Common.Extensions;
using Common.Telemetry;
using Ddei.Factories;
using DdeiClient.Enums;
using DdeiClient.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PolarisGateway.Mappers;
using PolarisGateway.TelemetryEvents;
using PolarisGateway.Validators;
using System.Net;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class ReclassifyDocument : BaseFunction
{
    private readonly ILogger<ReclassifyDocument> _logger;
    private readonly IDdeiClientFactory _ddeiClientFactory;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly IReclassifyDocumentRequestMapper _reclassifyDocumentRequestMapper;
    private readonly ITelemetryClient _telemetryClient;

    public ReclassifyDocument(
        ILogger<ReclassifyDocument> logger,
        IDdeiArgFactory ddeiArgFactory,
        IReclassifyDocumentRequestMapper reclassifyDocumentRequestMapper,
        ITelemetryClient telemetryClient,
        IDdeiClientFactory ddeiClientFactory)
        : base()
    {
        _logger = logger.ExceptionIfNull();
        _ddeiClientFactory = ddeiClientFactory.ExceptionIfNull();
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
        _reclassifyDocumentRequestMapper = reclassifyDocumentRequestMapper.ExceptionIfNull();
        _telemetryClient = telemetryClient.ExceptionIfNull();
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

            var ddeiClient = _ddeiClientFactory.Create(cmsAuthValues, DdeiClients.Mds);

            var result = await ddeiClient.ReclassifyDocumentAsync(arg);

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