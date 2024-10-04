using System.Net;
using Common.Configuration;
using Common.Dto.Request;
using Common.Extensions;
using Common.Telemetry;
using Common.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Handlers;
using PolarisGateway.Mappers;
using PolarisGateway.TelemetryEvents;
using PolarisGateway.Validators;

namespace PolarisGateway.Functions
{
    public class PolarisPipelineReclassifyDocument : BaseFunction
    {
        private readonly ILogger<PolarisPipelineReclassifyDocument> _logger;
        private readonly ICoordinatorClient _coordinatorClient;
        private readonly IReclassifyDocumentRequestMapper _reclassifyDocumentRequestMapper;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;
        private readonly ITelemetryClient _telemetryClient;

        public PolarisPipelineReclassifyDocument(
            ILogger<PolarisPipelineReclassifyDocument> logger,
            ICoordinatorClient coordinatorClient,
            IReclassifyDocumentRequestMapper reclassifyDocumentRequestMapper,
            IInitializationHandler initializationHandler,
            IUnhandledExceptionHandler unhandledExceptionHandler,
            ITelemetryClient telemetryClient)
            : base(telemetryClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _coordinatorClient = coordinatorClient ?? throw new ArgumentNullException(nameof(coordinatorClient));
            _reclassifyDocumentRequestMapper = reclassifyDocumentRequestMapper ?? throw new ArgumentNullException(nameof(reclassifyDocumentRequestMapper));
            _initializationHandler = initializationHandler ?? throw new ArgumentNullException(nameof(initializationHandler));
            _unhandledExceptionHandler = unhandledExceptionHandler ?? throw new ArgumentNullException(nameof(unhandledExceptionHandler));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        [Function(nameof(PolarisPipelineReclassifyDocument))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ReclassifyDocument)] HttpRequest req, string caseUrn, int caseId, string documentId)
        {
            var telemetryEvent = new DocumentReclassifiedEvent(caseId, documentId);

           (Guid CorrelationId, string? CmsAuthValues) context = default;

            try
            {
                context = await _initializationHandler.Initialize(req);
                telemetryEvent.IsRequestValid = true;
                telemetryEvent.CorrelationId = context.CorrelationId;

                var documentReclassification = await ValidatorHelper.GetJsonBody<DocumentReclassificationRequestDto, ReclassifyDocumentValidator>(req);
                var isRequestJsonValid = documentReclassification.IsValid;
                telemetryEvent.IsRequestJsonValid = isRequestJsonValid;
                telemetryEvent.RequestJson = documentReclassification.RequestJson;

                if (!isRequestJsonValid)
                {
                    return SendTelemetryAndReturnBadRequest(telemetryEvent);
                }

                var reclassifyDocumentDto = _reclassifyDocumentRequestMapper.Map(documentReclassification.Value);
                var response = await _coordinatorClient.ReclassifyDocument(
                    caseUrn,
                    caseId,
                    new PolarisDocumentId(documentId),
                    reclassifyDocumentDto,
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
                  nameof(PolarisPipelineReclassifyDocument),
                  context.CorrelationId,
                  ex
                );
            }
        }
    }
}