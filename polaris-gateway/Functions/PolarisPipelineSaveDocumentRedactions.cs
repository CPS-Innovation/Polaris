using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using PolarisGateway.Validators;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Mappers;
using Common.Dto.Request;
using Common.ValueObjects;
using Common.Telemetry;
using PolarisGateway.TelemetryEvents;
using System.Net;
using Common.Extensions;
using PolarisGateway.Handlers;
using FluentValidation;
using Newtonsoft.Json;

namespace PolarisGateway.Functions
{
    public class PolarisPipelineSaveDocumentRedactions : BaseFunction
    {
        private readonly IRedactPdfRequestMapper _redactPdfRequestMapper;
        private readonly ILogger<PolarisPipelineSaveDocumentRedactions> _logger;
        private readonly ICoordinatorClient _coordinatorClient;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;
        private readonly ITelemetryClient _telemetryClient;

        public PolarisPipelineSaveDocumentRedactions
            (
            IRedactPdfRequestMapper redactPdfRequestMapper,
            ICoordinatorClient coordinatorClient,
            ILogger<PolarisPipelineSaveDocumentRedactions> logger,
            IInitializationHandler initializationHandler,
            IUnhandledExceptionHandler unhandledExceptionHandler,
            ITelemetryClient telemetryClient)
        : base(telemetryClient)

        {
            _redactPdfRequestMapper = redactPdfRequestMapper ?? throw new ArgumentNullException(nameof(redactPdfRequestMapper));
            _coordinatorClient = coordinatorClient ?? throw new ArgumentNullException(nameof(coordinatorClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _initializationHandler = initializationHandler ?? throw new ArgumentNullException(nameof(initializationHandler));
            _unhandledExceptionHandler = unhandledExceptionHandler ?? throw new ArgumentNullException(nameof(unhandledExceptionHandler));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient)); ;
        }

        [FunctionName(nameof(PolarisPipelineSaveDocumentRedactions))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.Document)] HttpRequest req, string caseUrn, int caseId, string polarisDocumentId)
        {
            var telemetryEvent = new RedactionRequestEvent(caseId, polarisDocumentId);

            (Guid CorrelationId, string CmsAuthValues) context = default;
            try
            {
                context = await _initializationHandler.Initialize(req);
                telemetryEvent.IsRequestValid = true;
                telemetryEvent.CorrelationId = context.CorrelationId;

                var redactions = await GetJsonBody<DocumentRedactionSaveRequestDto, DocumentRedactionSaveRequestValidator>(req);
                var isRequestJsonValid = redactions.IsValid;
                telemetryEvent.IsRequestJsonValid = isRequestJsonValid;
                telemetryEvent.RequestJson = redactions.RequestJson;

                if (!isRequestJsonValid)
                {
                    // todo: log these errors to telemetry event
                    return SendTelemetryAndReturnBadRequest(telemetryEvent);
                }

                var redactPdfRequest = _redactPdfRequestMapper.Map(redactions.Value);
                var response = await _coordinatorClient.SaveRedactionsAsync(
                    caseUrn,
                    caseId,
                    new PolarisDocumentId(polarisDocumentId),
                    redactPdfRequest,
                    context.CmsAuthValues,
                    context.CorrelationId);

                telemetryEvent.IsSuccess = response.IsSuccessStatusCode();

                return SendTelemetryAndReturn(telemetryEvent, response);
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackEventFailure(telemetryEvent);
                return _unhandledExceptionHandler.HandleUnhandledException(
                  _logger,
                  nameof(PolarisPipelineSaveDocumentRedactions),
                  context.CorrelationId,
                  ex
                );
            }
        }
    }
}

