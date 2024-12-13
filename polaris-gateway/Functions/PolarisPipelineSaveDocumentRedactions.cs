using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using PolarisGateway.Validators;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Mappers;
using Common.Dto.Request;
using Common.Telemetry;
using PolarisGateway.TelemetryEvents;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Common.Wrappers;
using PolarisGateway.Helpers;
using System.Threading.Tasks;
using System.Net.Http;
using System;

namespace PolarisGateway.Functions;

public class PolarisPipelineSaveDocumentRedactions : BaseFunction
{
    private readonly IRedactPdfRequestMapper _redactPdfRequestMapper;
    private readonly ILogger<PolarisPipelineSaveDocumentRedactions> _logger;
    private readonly ICoordinatorClient _coordinatorClient;
    private readonly ITelemetryClient _telemetryClient;
    private readonly IJsonConvertWrapper _jsonConvertWrapper;

    public PolarisPipelineSaveDocumentRedactions(
        IRedactPdfRequestMapper redactPdfRequestMapper,
        ICoordinatorClient coordinatorClient,
        ILogger<PolarisPipelineSaveDocumentRedactions> logger,
        ITelemetryClient telemetryClient,
        IJsonConvertWrapper jsonConvertWrapper)
        : base(telemetryClient)

    {
        _redactPdfRequestMapper = redactPdfRequestMapper ?? throw new ArgumentNullException(nameof(redactPdfRequestMapper));
        _coordinatorClient = coordinatorClient ?? throw new ArgumentNullException(nameof(coordinatorClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
    }

    [Function(nameof(PolarisPipelineSaveDocumentRedactions))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.RedactDocument)] HttpRequest req, string caseUrn, int caseId, string documentId, long versionId)
    {
        var telemetryEvent = new RedactionRequestEvent(caseId, documentId);

        var correlationId = EstablishCorrelation(req);
        var cmsAuthValues = EstablishCmsAuthValues(req);

        try
        {
            telemetryEvent.IsRequestValid = true;
            telemetryEvent.CorrelationId = correlationId;

            var redactions = await RequestHelper.GetJsonBody<DocumentRedactionSaveRequestDto, DocumentRedactionSaveRequestValidator>(req);
            var isRequestJsonValid = redactions.IsValid;
            telemetryEvent.IsRequestJsonValid = isRequestJsonValid;
            telemetryEvent.RequestJson = redactions.RequestJson;

            if (!isRequestJsonValid)
            {
                // todo: log these errors to telemetry event
                return await SendTelemetryAndReturn(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.BadRequest
                },
                telemetryEvent);
            }

            var redactPdfRequest = _redactPdfRequestMapper.Map(redactions.Value);
            var response = await _coordinatorClient.SaveRedactionsAsync(
                caseUrn,
                caseId,
                documentId,
                versionId,
                redactPdfRequest,
                cmsAuthValues,
                correlationId);

            telemetryEvent.IsSuccess = response.IsSuccessStatusCode;
            telemetryEvent.DeletedPageCount = redactPdfRequest.DocumentModifications.Count;

            return await SendTelemetryAndReturn(response, telemetryEvent);
        }
        catch
        {
            _telemetryClient.TrackEventFailure(telemetryEvent);
            throw;
        }
    }
}

