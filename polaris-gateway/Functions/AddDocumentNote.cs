using Common.Configuration;
using Common.Dto.Request;
using Common.Extensions;
using Common.Telemetry;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PolarisGateway.Helpers;
using PolarisGateway.TelemetryEvents;
using PolarisGateway.Validators;
using System.Net;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class AddDocumentNote : BaseFunction
{
    private readonly ILogger<AddDocumentNote> _logger;
    private readonly IMdsArgFactory _mdsArgFactory;
    private readonly ITelemetryClient _telemetryClient;
    private readonly IMdsClient _mdsClient;

    public AddDocumentNote(
        ILogger<AddDocumentNote> logger,
        IMdsArgFactory mdsArgFactory,
        ITelemetryClient telemetryClient, 
        IMdsClient mdsClient)
    {
        _logger = logger.ExceptionIfNull();
        _mdsArgFactory = mdsArgFactory.ExceptionIfNull();
        _telemetryClient = telemetryClient.ExceptionIfNull();
        _mdsClient = mdsClient.ExceptionIfNull();
    }

    [Function(nameof(AddDocumentNote))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.DocumentNotes)] HttpRequest req,
        string caseUrn,
        int caseId,
        string documentId)
    {
        var telemetryEvent = new DocumentNoteRequestEvent(caseId, documentId)
        {
            OperationName = nameof(AddDocumentNote),
        };
        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        try
        {
            telemetryEvent.IsRequestValid = true;
            telemetryEvent.CorrelationId = correlationId;

            var body = await RequestHelper.GetJsonBody<AddDocumentNoteRequestDto, AddDocumentNoteValidator>(req);
            telemetryEvent.IsRequestJsonValid = body.IsValid;
            telemetryEvent.RequestJson = body.RequestJson;

            if (!body.IsValid)
            {
                _telemetryClient.TrackEvent(telemetryEvent);
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            var arg = _mdsArgFactory.CreateAddDocumentNoteArgDto(cmsAuthValues, correlationId, caseUrn, caseId, documentId, body.Value.Text);
            await _mdsClient.AddDocumentNoteAsync(arg);

            telemetryEvent.IsSuccess = true;
            _telemetryClient.TrackEvent(telemetryEvent);

            return new OkResult();
        }
        catch
        {
            _telemetryClient.TrackEventFailure(telemetryEvent);
            throw;
        }
    }
}