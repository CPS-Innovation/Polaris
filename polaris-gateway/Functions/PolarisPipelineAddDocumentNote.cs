using System.Net;
using Common.Configuration;
using Common.Dto.Request;
using Common.Telemetry;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Handlers;
using PolarisGateway.Mappers;
using PolarisGateway.TelemetryEvents;
using PolarisGateway.Validators;

namespace PolarisGateway.Functions
{
    public class PolarisPipelineAddDocumentNote
    {
        private readonly ILogger<PolarisPipelineAddDocumentNote> _logger;
        private readonly IDocumentNoteRequestMapper _documentNoteRequestMapper;
        private readonly ICoordinatorClient _coordinatorClient;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;
        private readonly ITelemetryClient _telemetryClient;

        public PolarisPipelineAddDocumentNote(ILogger<PolarisPipelineAddDocumentNote> logger,
            IDocumentNoteRequestMapper documentNoteRequestMapper,
            ICoordinatorClient coordinatorClient,
            IInitializationHandler initializationHandler,
            IUnhandledExceptionHandler unhandledExceptionHandler,
            ITelemetryClient telemetryClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _documentNoteRequestMapper = documentNoteRequestMapper;
            _coordinatorClient = coordinatorClient ?? throw new ArgumentNullException(nameof(coordinatorClient));
            _initializationHandler = initializationHandler ?? throw new ArgumentNullException(nameof(initializationHandler));
            _unhandledExceptionHandler = unhandledExceptionHandler ?? throw new ArgumentNullException(nameof(unhandledExceptionHandler));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        [FunctionName(nameof(PolarisPipelineAddDocumentNote))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.AddNoteToDocument)] HttpRequest req,
            string caseUrn,
            int caseId,
            int documentId)
        {
            (Guid CorrelationId, string CmsAuthValues) context = default;

            var telemetryEvent = new DocumentNoteRequestEvent(caseId, documentId.ToString());

            HttpResponseMessage SendTelemetryAndReturn(HttpResponseMessage result)
            {
                _telemetryClient.TrackEvent(telemetryEvent);
                return result;
            }

            try
            {
                context = await _initializationHandler.Initialize(req);
                telemetryEvent.IsRequestValid = true;
                telemetryEvent.CorrelationId = context.CorrelationId;

                var documentNoteRequest = await GetJsonBody<AddDocumentNoteRequestDto, AddDocumentNoteValidator>(req);
                var isRequestJsonValid = documentNoteRequest.IsValid;
                telemetryEvent.IsRequestJsonValid = isRequestJsonValid;
                telemetryEvent.RequestJson = documentNoteRequest.RequestJson;

                if (!isRequestJsonValid)
                {
                    // todo: log these errors to telemetry event
                    return SendTelemetryAndReturn(new HttpResponseMessage()
                    {
                        StatusCode = HttpStatusCode.BadRequest
                    });
                }

                var documentNote = _documentNoteRequestMapper.Map(documentNoteRequest.Value);
                var response = await _coordinatorClient.AddDocumentNote(
                    caseUrn,
                    caseId,
                    context.CmsAuthValues,
                    documentId,
                    documentNote,
                    context.CorrelationId);

                telemetryEvent.IsSuccess = response.IsSuccessStatusCode;

                return SendTelemetryAndReturn(response);
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackEventFailure(telemetryEvent);
                return _unhandledExceptionHandler.HandleUnhandledException(
                      _logger,
                      nameof(PolarisPipelineAddDocumentNote),
                      context.CorrelationId,
                      ex
                    );
            }
        }

        public static async Task<ValidatableRequest<T>> GetJsonBody<T, V>(HttpRequest request) where V : AbstractValidator<T>, new()
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