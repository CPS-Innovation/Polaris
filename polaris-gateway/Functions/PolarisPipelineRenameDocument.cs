using System.Net;
using Common.Configuration;
using Common.Dto.Request;
using Common.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Handlers;
using PolarisGateway.Helpers;
using PolarisGateway.TelemetryEvents;
using PolarisGateway.Validators;

namespace PolarisGateway.Functions
{
    public class PolarisPipelineRenameDocument
    {
        private readonly ILogger<PolarisPipelineRenameDocument> _logger;
        private readonly ICoordinatorClient _coordinatorClient;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;
        private readonly ITelemetryClient _telemetryClient;

        public PolarisPipelineRenameDocument(ILogger<PolarisPipelineRenameDocument> logger,
            ICoordinatorClient coordinatorClient,
            IInitializationHandler initializationHandler,
            IUnhandledExceptionHandler unhandledExceptionHandler,
            ITelemetryClient telemetryClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _coordinatorClient = coordinatorClient ?? throw new ArgumentNullException(nameof(coordinatorClient));
            _initializationHandler = initializationHandler ?? throw new ArgumentNullException(nameof(initializationHandler));
            _unhandledExceptionHandler = unhandledExceptionHandler ?? throw new ArgumentNullException(nameof(unhandledExceptionHandler));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        [FunctionName(nameof(PolarisPipelineRenameDocument))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.RenameDocument)] HttpRequest req, string caseUrn, int caseId, string documentId)
        {
            var telemetryEvent = new RenameDocumentRequestEvent(caseId, documentId.ToString());

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

                var nameChange = await RequestHelper.GetJsonBody<RenameDocumentRequestDto, RenameDocumentRequestValidator>(req);
                var isRequestJsonValid = nameChange.IsValid;
                telemetryEvent.IsRequestJsonValid = isRequestJsonValid;
                telemetryEvent.RequestJson = nameChange.RequestJson;

                if (!isRequestJsonValid)
                {
                    // todo: log these errors to telemetry event
                    return SendTelemetryAndReturn(new HttpResponseMessage()
                    {
                        StatusCode = HttpStatusCode.BadRequest
                    });
                }

                var response = await _coordinatorClient.RenameDocumentAsync(
                    caseUrn,
                    caseId,
                    context.CmsAuthValues,
                    documentId,
                    nameChange.Value,
                    context.CorrelationId);

                telemetryEvent.IsSuccess = response.IsSuccessStatusCode;

                return SendTelemetryAndReturn(response);
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackEventFailure(telemetryEvent);
                return _unhandledExceptionHandler.HandleUnhandledException(
                    _logger,
                    nameof(PolarisPipelineRenameDocument),
                    context.CorrelationId,
                    ex
                );
            }
        }
    }
}