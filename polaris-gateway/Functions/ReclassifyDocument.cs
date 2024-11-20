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
using PolarisGateway.Mappers;
using PolarisGateway.TelemetryEvents;
using PolarisGateway.Validators;

namespace PolarisGateway.Functions
{
    public class ReclassifyDocument
    {
        private readonly ILogger<ReclassifyDocument> _logger;
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;
        private readonly IReclassifyDocumentRequestMapper _reclassifyDocumentRequestMapper;
        private readonly IInitializationHandler _initializationHandler;
        private readonly IUnhandledExceptionHandler _unhandledExceptionHandler;
        private readonly ITelemetryClient _telemetryClient;

        public ReclassifyDocument(
            ILogger<ReclassifyDocument> logger,
            IDdeiClient ddeiClient,
            IDdeiArgFactory ddeiArgFactory,
            IReclassifyDocumentRequestMapper reclassifyDocumentRequestMapper,
            IInitializationHandler initializationHandler,
            IUnhandledExceptionHandler unhandledExceptionHandler,
            ITelemetryClient telemetryClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
            _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
            _reclassifyDocumentRequestMapper = reclassifyDocumentRequestMapper ?? throw new ArgumentNullException(nameof(reclassifyDocumentRequestMapper));
            _initializationHandler = initializationHandler ?? throw new ArgumentNullException(nameof(initializationHandler));
            _unhandledExceptionHandler = unhandledExceptionHandler ?? throw new ArgumentNullException(nameof(unhandledExceptionHandler));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
        }

        [FunctionName(nameof(ReclassifyDocument))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ReclassifyDocument)] HttpRequest req, string caseUrn, int caseId, string documentId)
        {
            var telemetryEvent = new DocumentReclassifiedEvent(caseId, documentId);

            (Guid CorrelationId, string CmsAuthValues) context = default;

            try
            {
                context = await _initializationHandler.Initialize(req);
                telemetryEvent.IsRequestValid = true;
                telemetryEvent.CorrelationId = context.CorrelationId;

                var body = await ValidatorHelper.GetJsonBody<ReclassifyDocumentDto, ReclassifyDocumentValidator>(req);
                telemetryEvent.IsRequestJsonValid = body.IsValid;
                telemetryEvent.RequestJson = body.RequestJson;

                if (!body.IsValid)
                {
                    _telemetryClient.TrackEvent(telemetryEvent);
                    return new StatusCodeResult((int)HttpStatusCode.BadRequest);
                }

                var arg = _ddeiArgFactory.CreateReclassifyDocumentArgDto
                (
                    cmsAuthValues: context.CmsAuthValues,
                    correlationId: context.CorrelationId,
                    urn: caseUrn,
                    caseId: caseId,
                    documentId: documentId,
                    dto: body.Value
                );

                var result = await _ddeiClient.ReclassifyDocumentAsync(arg);

                telemetryEvent.IsSuccess = true;
                _telemetryClient.TrackEvent(telemetryEvent);

                return new ObjectResult(result);
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackEventFailure(telemetryEvent);
                return _unhandledExceptionHandler.HandleUnhandledExceptionActionResult(
                  _logger,
                  nameof(ReclassifyDocument),
                  context.CorrelationId,
                  ex
                );
            }
        }
    }
}