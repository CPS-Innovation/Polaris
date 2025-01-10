using System;
using System.Net;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Dto.Request;
using Common.Telemetry;
using Common.Wrappers;
using Ddei;
using Ddei.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PolarisGateway.Helpers;
using PolarisGateway.TelemetryEvents;
using PolarisGateway.Validators;

namespace PolarisGateway.Functions;

public class AddDocumentNote : BaseFunction
{
    private readonly ILogger<AddDocumentNote> _logger;
    private readonly IDdeiClient _ddeiClient;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly ITelemetryClient _telemetryClient;
    private readonly IJsonConvertWrapper _jsonConvertWrapper;

    public AddDocumentNote(
        ILogger<AddDocumentNote> logger,
        IDdeiClient ddeiClient,
        IDdeiArgFactory ddeiArgFactory,
        ITelemetryClient telemetryClient,
        IJsonConvertWrapper jsonConvertWrapper)
        : base()
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
        _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
        _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
    }

    [Function(nameof(AddDocumentNote))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.DocumentNotes)] HttpRequest req,
        string caseUrn,
        int caseId,
        string documentId)
    {
        var telemetryEvent = new DocumentNoteRequestEvent(caseId, documentId.ToString())
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

            var arg = _ddeiArgFactory.CreateAddDocumentNoteArgDto(cmsAuthValues, correlationId, caseUrn, caseId, documentId, body.Value.Text);
            var result = await _ddeiClient.AddDocumentNoteAsync(arg);

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