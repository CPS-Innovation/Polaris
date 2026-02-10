using Common.Configuration;
using Common.Dto.Request;
using Common.Extensions;
using Common.Telemetry;
using Ddei.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PolarisGateway.Services.DdeiOrchestration;
using PolarisGateway.TelemetryEvents;
using PolarisGateway.Validators;
using System.Net;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class ReclassifyDocument : BaseFunction
{
    private readonly ILogger<ReclassifyDocument> _logger;
    private readonly IMdsArgFactory _mdsArgFactory;
    private readonly IDdeiReclassifyDocumentOrchestrationService _ddeiOrchestrationService;
    private readonly ITelemetryClient _telemetryClient;

    public ReclassifyDocument(
        ILogger<ReclassifyDocument> logger,
        IMdsArgFactory mdsArgFactory,
        ITelemetryClient telemetryClient,
        IDdeiReclassifyDocumentOrchestrationService ddeiOrchestrationService)
        : base()
    {
        _logger = logger.ExceptionIfNull();
        _mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        _telemetryClient = telemetryClient.ExceptionIfNull();
        _ddeiOrchestrationService = ddeiOrchestrationService.ExceptionIfNull();
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

            var arg = _mdsArgFactory.CreateReclassifyDocumentArgDto
            (
                cmsAuthValues: cmsAuthValues,
                correlationId: correlationId,
                urn: caseUrn,
                caseId: caseId,
                documentId: documentId,
                dto: body.Value
            );

            var reclassifyDocumentResult = await _ddeiOrchestrationService.ReclassifyDocument(arg);

            if (!reclassifyDocumentResult.IsSuccess)
            {
                telemetryEvent.IsSuccess = false;
                _telemetryClient.TrackEvent(telemetryEvent);
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }

            telemetryEvent.IsSuccess = true;
            telemetryEvent.ResponseDocumentId = (int)reclassifyDocumentResult.Result.DocumentId;
            telemetryEvent.ReclassificationType = reclassifyDocumentResult.Result.ReclassificationType;
            telemetryEvent.OriginalDocumentTypeId = reclassifyDocumentResult.Result.OriginalDocumentTypeId;
            telemetryEvent.NewDocumentTypeId = reclassifyDocumentResult.Result.DocumentTypeId;
            telemetryEvent.DocumentRenamed = reclassifyDocumentResult.Result.DocumentRenamed;
            telemetryEvent.DocumentRenameOperationName = reclassifyDocumentResult.Result.DocumentRenamedOperationName;
            _telemetryClient.TrackEvent(telemetryEvent);

            return new ObjectResult(reclassifyDocumentResult.Result);
        }
        catch
        {
            _telemetryClient.TrackEventFailure(telemetryEvent);
            throw;
        }
    }
}