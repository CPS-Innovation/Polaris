using System.Net;
using Common.Configuration;
using Common.Dto.Request;
using Common.Extensions;
using Common.Telemetry;
using Common.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Handlers;
using PolarisGateway.Mappers;
using PolarisGateway.TelemetryEvents;
using PolarisGateway.Validators;

namespace PolarisGateway.Functions
{
    public class PolarisPipelineModifyDocument : BaseFunction
    {
        private readonly ILogger<PolarisPipelineModifyDocument> _logger;
        private readonly ICoordinatorClient _coordinatorClient;
        private readonly IModifyDocumentRequestMapper _modifyDocumentRequestMapper;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;
        private readonly ITelemetryClient _telemetryClient;

        public PolarisPipelineModifyDocument(
            ILogger<PolarisPipelineModifyDocument> logger,
            ICoordinatorClient coordinatorClient,
            IModifyDocumentRequestMapper modifyDocumentRequestMapper,
            IInitializationHandler initializationHandler,
            IUnhandledExceptionHandler unhandledExceptionHandler,
            ITelemetryClient telemetryClient)
        : base(telemetryClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _coordinatorClient = coordinatorClient ?? throw new ArgumentNullException(nameof(coordinatorClient));
            _modifyDocumentRequestMapper = modifyDocumentRequestMapper ?? throw new ArgumentNullException(nameof(modifyDocumentRequestMapper));
            _initializationHandler = initializationHandler ?? throw new ArgumentNullException(nameof(initializationHandler));
            _unhandledExceptionHandler = unhandledExceptionHandler ?? throw new ArgumentNullException(nameof(unhandledExceptionHandler));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        [FunctionName(nameof(PolarisPipelineModifyDocument))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ModifyDocument)] HttpRequest req, string caseUrn, int caseId, string documentId)
        {
            var telemetryEvent = new DocumentModifiedEvent(caseId, documentId);

            (Guid CorrelationId, string CmsAuthValues) context = default;

            try
            {
                context = await _initializationHandler.Initialize(req);
                telemetryEvent.IsRequestValid = true;
                telemetryEvent.CorrelationId = context.CorrelationId;

                var documentChanges = await ValidatorHelper.GetJsonBody<DocumentModificationRequestDto, ModifyDocumentPagesValidator>(req);
                var isRequestJsonValid = documentChanges.IsValid;
                telemetryEvent.IsRequestJsonValid = isRequestJsonValid;
                telemetryEvent.RequestJson = documentChanges.RequestJson;

                if (!isRequestJsonValid)
                    return SendTelemetryAndReturnBadRequest(telemetryEvent);
                
                var modifyDocumentDto = _modifyDocumentRequestMapper.Map(documentChanges.Value);
                var response = await _coordinatorClient.ModifyDocument(
                    caseUrn,
                    caseId,
                    new PolarisDocumentId(documentId),
                    modifyDocumentDto,
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
                  nameof(PolarisPipelineModifyDocument),
                  context.CorrelationId,
                  ex
                );
            }
        }
    }
}