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
using PolarisGateway.Helpers;
using PolarisGateway.TelemetryEvents;
using PolarisGateway.Validators;
using System.Net;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class AddDocumentNote : BaseFunction
{
    private readonly ILogger<AddDocumentNote> _logger;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly ITelemetryClient _telemetryClient;
    private readonly IDdeiClientFactory _ddeiClientFactory;

    public AddDocumentNote(
        ILogger<AddDocumentNote> logger,
        IDdeiArgFactory ddeiArgFactory,
        ITelemetryClient telemetryClient, IDdeiClientFactory ddeiClientFactory)
        : base()
    {
        _logger = logger.ExceptionIfNull();
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
        _telemetryClient = telemetryClient.ExceptionIfNull();
        _ddeiClientFactory = ddeiClientFactory.ExceptionIfNull();
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

            var arg = _ddeiArgFactory.CreateAddDocumentNoteArgDto(cmsAuthValues, correlationId, caseUrn, caseId, documentId, body.Value.Text);
            var ddeiClient = _ddeiClientFactory.Create(cmsAuthValues, DdeiClients.Mds);
            var result = await ddeiClient.AddDocumentNoteAsync(arg);

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