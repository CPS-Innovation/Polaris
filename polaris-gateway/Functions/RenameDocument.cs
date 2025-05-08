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

public class RenameDocument : BaseFunction
{
    private readonly ILogger<RenameDocument> _logger;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly ITelemetryClient _telemetryClient;
    private readonly IDdeiClientFactory _ddeiClientFactory;

    public RenameDocument(ILogger<RenameDocument> logger,
        IDdeiArgFactory ddeiArgFactory,
        ITelemetryClient telemetryClient,
        IDdeiClientFactory ddeiClientFactory)
    {
        _logger = logger.ExceptionIfNull();
        _ddeiArgFactory = ddeiArgFactory.ExceptionIfNull();
        _telemetryClient = telemetryClient.ExceptionIfNull();
        _ddeiClientFactory = ddeiClientFactory.ExceptionIfNull();
    }

    [Function(nameof(RenameDocument))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.RenameDocument)] HttpRequest req, string caseUrn, int caseId, string documentId)
    {
        var telemetryEvent = new RenameDocumentRequestEvent(caseId, documentId)
        {
            OperationName = nameof(RenameDocument),
        };

        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        try
        {
            telemetryEvent.IsRequestValid = true;
            telemetryEvent.CorrelationId = correlationId;

            var body = await RequestHelper.GetJsonBody<RenameDocumentRequestDto, RenameDocumentRequestValidator>(req);
            var isRequestJsonValid = body.IsValid;
            telemetryEvent.IsRequestJsonValid = isRequestJsonValid;
            telemetryEvent.RequestJson = body.RequestJson;

            if (!isRequestJsonValid)
            {
                _telemetryClient.TrackEvent(telemetryEvent);
                return new StatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            var arg = _ddeiArgFactory.CreateRenameDocumentArgDto(cmsAuthValues, correlationId, caseUrn, caseId, documentId, body.Value.DocumentName);
            var ddeiClient = _ddeiClientFactory.Create(cmsAuthValues, DdeiClients.Mds);
            await ddeiClient.RenameDocumentAsync(arg);

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