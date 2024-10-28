using System.Net;
using Common.Configuration;
using Common.Dto.Request;
using Common.Telemetry;
using Ddei;
using Ddei.Factories;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PolarisGateway.Handlers;
using PolarisGateway.TelemetryEvents;
using PolarisGateway.Validators;

namespace PolarisGateway.Functions
{
    public class AddDocumentNote
    {
        private readonly ILogger<AddDocumentNote> _logger;
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;
        private readonly ITelemetryClient _telemetryClient;

        public AddDocumentNote(
            ILogger<AddDocumentNote> logger,
            IDdeiClient ddeiClient,
            IDdeiArgFactory ddeiArgFactory,
            IInitializationHandler initializationHandler,
            IUnhandledExceptionHandler unhandledExceptionHandler,
            ITelemetryClient telemetryClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
            _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
            _initializationHandler = initializationHandler ?? throw new ArgumentNullException(nameof(initializationHandler));
            _unhandledExceptionHandler = unhandledExceptionHandler ?? throw new ArgumentNullException(nameof(unhandledExceptionHandler));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        [FunctionName(nameof(AddDocumentNote))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.DocumentNotes)] HttpRequest req,
            string caseUrn,
            int caseId,
            string documentId)
        {
            (Guid CorrelationId, string CmsAuthValues) context = default;

            var telemetryEvent = new DocumentNoteRequestEvent(caseId, documentId.ToString());

            try
            {
                context = await _initializationHandler.Initialize(req);

                telemetryEvent.IsRequestValid = true;
                telemetryEvent.CorrelationId = context.CorrelationId;
                var body = await GetJsonBody<AddDocumentNoteRequestDto, AddDocumentNoteValidator>(req);
                telemetryEvent.IsRequestJsonValid = body.IsValid;
                telemetryEvent.RequestJson = body.RequestJson;

                if (!body.IsValid)
                {
                    _telemetryClient.TrackEvent(telemetryEvent);
                    return new StatusCodeResult((int)HttpStatusCode.BadRequest);
                }

                var arg = _ddeiArgFactory.CreateAddDocumentNoteArgDto(context.CmsAuthValues, context.CorrelationId, caseUrn, caseId, documentId, body.Value.Text);
                var result = await _ddeiClient.AddDocumentNoteAsync(arg);

                telemetryEvent.IsSuccess = true;
                _telemetryClient.TrackEvent(telemetryEvent);

                return new OkResult();
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackEventFailure(telemetryEvent);
                return _unhandledExceptionHandler.HandleUnhandledExceptionActionResult(
                      _logger,
                      nameof(AddDocumentNote),
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