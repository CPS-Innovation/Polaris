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
using Common.Telemetry.Contracts;
using PolarisGateway.TelemetryEvents;
using System.Net;
using PolarisGateway.Handlers;
using FluentValidation;
using Newtonsoft.Json;

namespace PolarisGateway.Functions
{
    public class PolarisPipelineSaveDocumentRedactions
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
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.Document)] HttpRequest req, string caseUrn, int caseId, string polarisDocumentId)
        {
            var telemetryEvent = new RedactionRequestEvent(caseId, polarisDocumentId);

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

                var redactions = await GetJsonBody<DocumentRedactionSaveRequestDto, DocumentRedactionSaveRequestValidator>(req);
                var isRequestJsonValid = redactions.IsValid;
                telemetryEvent.IsRequestJsonValid = isRequestJsonValid;
                telemetryEvent.RequestJson = redactions.RequestJson;

                if (!isRequestJsonValid)
                {
                    // todo: log these errors to telemetry event
                    return SendTelemetryAndReturn(new HttpResponseMessage()
                    {
                        StatusCode = HttpStatusCode.BadRequest
                    });
                }

                var redactPdfRequest = _redactPdfRequestMapper.Map(redactions.Value);
                var response = await _coordinatorClient.SaveRedactionsAsync(
                    caseUrn,
                    caseId,
                    new PolarisDocumentId(polarisDocumentId),
                    redactPdfRequest,
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
                  nameof(PolarisPipelineSaveDocumentRedactions),
                  context.CorrelationId,
                  ex
                );
            }
        }

        public static async Task<ValidatableRequest<T>> GetJsonBody<T, V>(HttpRequest request)
    where V : AbstractValidator<T>, new()
        {
            var requestJson = await request.ReadAsStringAsync();
            var requestObject = JsonConvert.DeserializeObject<T>(requestJson);

            var validator = new V();
            var validationResult = await validator.ValidateAsync(requestObject);

            return new ValidatableRequest<T>
            {
                Value = requestObject,
                IsValid = validationResult.IsValid,
                RequestJson = requestJson
            };
        }
    }
}

