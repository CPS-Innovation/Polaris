using System.Net;
using Common.Configuration;
using Common.Dto.Request;
using Common.Telemetry;
using Ddei;
using Ddei.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PolarisGateway.Handlers;
using PolarisGateway.Helpers;
using PolarisGateway.TelemetryEvents;
using PolarisGateway.Validators;

namespace PolarisGateway.Functions
{
    public class RenameDocument
    {
        private readonly ILogger<RenameDocument> _logger;
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;
        private readonly ITelemetryClient _telemetryClient;

        public RenameDocument(ILogger<RenameDocument> logger,
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

        [FunctionName(nameof(RenameDocument))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.RenameDocument)] HttpRequest req, string caseUrn, int caseId, string documentId)
        {
            var telemetryEvent = new RenameDocumentRequestEvent(caseId, documentId.ToString());

            (Guid CorrelationId, string CmsAuthValues) context = default;
            try
            {
                context = await _initializationHandler.Initialize(req);
                telemetryEvent.IsRequestValid = true;
                telemetryEvent.CorrelationId = context.CorrelationId;

                var body = await RequestHelper.GetJsonBody<RenameDocumentRequestDto, RenameDocumentRequestValidator>(req);
                var isRequestJsonValid = body.IsValid;
                telemetryEvent.IsRequestJsonValid = isRequestJsonValid;
                telemetryEvent.RequestJson = body.RequestJson;

                if (!isRequestJsonValid)
                {
                    _telemetryClient.TrackEvent(telemetryEvent);
                    return new StatusCodeResult((int)HttpStatusCode.BadRequest);
                }

                var arg = _ddeiArgFactory.CreateRenameDocumentArgDto(context.CmsAuthValues, context.CorrelationId, caseUrn, caseId, documentId, body.Value.DocumentName);
                var result = await _ddeiClient.RenameDocumentAsync(arg);

                telemetryEvent.IsSuccess = true;
                _telemetryClient.TrackEvent(telemetryEvent);

                return new OkResult();
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackEventFailure(telemetryEvent);
                return _unhandledExceptionHandler.HandleUnhandledExceptionActionResult(
                    _logger,
                    nameof(RenameDocument),
                    context.CorrelationId,
                    ex
                );
            }
        }
    }
}