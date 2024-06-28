using System.Net;
using Common.Configuration;
using Common.Dto.Request;
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
    public class PolarisPipelineRemoveDocumentPages
    {
        private readonly ILogger<PolarisPipelineRemoveDocumentPages> _logger;
        private readonly ICoordinatorClient _coordinatorClient;
        private readonly IRemoveDocumentPagesRequestMapper _removeDocumentPagesRequestMapper;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;
        private readonly ITelemetryClient _telemetryClient;

        public PolarisPipelineRemoveDocumentPages(
            ILogger<PolarisPipelineRemoveDocumentPages> logger,
            ICoordinatorClient coordinatorClient,
            IRemoveDocumentPagesRequestMapper removeDocumentPagesRequestMapper,
            IInitializationHandler initializationHandler,
            IUnhandledExceptionHandler unhandledExceptionHandler,
            ITelemetryClient telemetryClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _coordinatorClient = coordinatorClient ?? throw new ArgumentNullException(nameof(coordinatorClient));
            _removeDocumentPagesRequestMapper = removeDocumentPagesRequestMapper ?? throw new ArgumentNullException(nameof(removeDocumentPagesRequestMapper));
            _initializationHandler = initializationHandler ?? throw new ArgumentNullException(nameof(initializationHandler));
            _unhandledExceptionHandler = unhandledExceptionHandler ?? throw new ArgumentNullException(nameof(unhandledExceptionHandler));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        [FunctionName(nameof(PolarisPipelineRemoveDocumentPages))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.RemoveDocumentPages)] HttpRequest req, string caseUrn, int caseId, string polarisDocumentId)
        {
            var telemetryEvent = new RemoveDocumentPageEvent(caseId, polarisDocumentId);

            HttpResponseMessage SendTelemetryAndReturn(HttpResponseMessage result)
            {
                _telemetryClient.TrackEvent(telemetryEvent);
                return result;
            }

            (Guid CorrelationId, string CmsAuthValues) context = default;

            try
            {
                context = await _initializationHandler.Initialize(req);
                telemetryEvent.IsRequestValid = true;
                telemetryEvent.CorrelationId = context.CorrelationId;

                var pageIndexes = await ValidatorHelper.GetJsonBody<DocumentPageRemovalRequestDto, RemoveDocumentPagesValidator>(req);
                var isRequestJsonValid = pageIndexes.IsValid;
                telemetryEvent.IsRequestJsonValid = isRequestJsonValid;
                telemetryEvent.RequestJson = pageIndexes.RequestJson;

                if (!isRequestJsonValid)
                {
                    return SendTelemetryAndReturn(new HttpResponseMessage()
                    {
                        StatusCode = HttpStatusCode.BadRequest
                    });
                }

                var removeDocumentPagesDto = _removeDocumentPagesRequestMapper.Map(pageIndexes.Value);
                var response = await _coordinatorClient.RemoveDocumentPages(
                    caseUrn,
                    caseId,
                    new PolarisDocumentId(polarisDocumentId),
                    removeDocumentPagesDto,
                    context.CmsAuthValues,
                    context.CorrelationId);

                telemetryEvent.IsSuccess = response.IsSuccessStatusCode;

                return SendTelemetryAndReturn(response);
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackEventFailure(telemetryEvent);
                return _unhandledExceptionHandler.HandleUnhandledException(
                  _logger,
                  nameof(PolarisPipelineRemoveDocumentPages),
                  context.CorrelationId,
                  ex
                );
            }
        }
    }
}