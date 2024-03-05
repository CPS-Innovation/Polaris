using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using PolarisGateway.Domain.Validators;
using PolarisGateway.Clients;
using PolarisGateway.common.Mappers;
using Common.Telemetry.Wrappers.Contracts;
using Common.Dto.Request;
using Common.ValueObjects;
using Common.Telemetry.Contracts;
using PolarisGateway.TelemetryEvents;
using Common.Extensions;

namespace PolarisGateway.Functions
{
    public class PolarisPipelineSaveDocumentRedactions : BasePolarisFunction
    {
        private readonly IRedactPdfRequestMapper _redactPdfRequestMapper;
        private readonly IPipelineClient _pipelineClient;
        private readonly ILogger<PolarisPipelineSaveDocumentRedactions> _logger;
        private readonly ITelemetryClient _telemetryClient;

        public PolarisPipelineSaveDocumentRedactions
            (
                IRedactPdfRequestMapper redactPdfRequestMapper,
                IPipelineClient pipelineClient,
                ILogger<PolarisPipelineSaveDocumentRedactions> logger,
                IAuthorizationValidator tokenValidator,
                ITelemetryAugmentationWrapper telemetryAugmentationWrapper,
                ITelemetryClient telemetryClient
            )

        : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _redactPdfRequestMapper = redactPdfRequestMapper ?? throw new ArgumentNullException(nameof(redactPdfRequestMapper));
            _pipelineClient = pipelineClient ?? throw new ArgumentNullException(nameof(pipelineClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient)); ;
        }

        [FunctionName(nameof(PolarisPipelineSaveDocumentRedactions))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.Document)] HttpRequest req, string caseUrn, int caseId, string polarisDocumentId)
        {
            var telemetryEvent = new RedactionRequestEvent(caseId, polarisDocumentId);

            IActionResult sendTelemetryAndReturn(IActionResult result)
            {
                _telemetryClient.TrackEvent(telemetryEvent);
                return result;
            }

            try
            {
                await Initiate(req);
                telemetryEvent.IsRequestValid = true;
                telemetryEvent.CorrelationId = CorrelationId;
                var redactions = await req.GetJsonBody<DocumentRedactionSaveRequestDto, DocumentRedactionSaveRequestValidator>();
                var isRequestJsonValid = redactions.IsValid;
                telemetryEvent.IsRequestJsonValid = isRequestJsonValid;
                telemetryEvent.RequestJson = redactions.RequestJson;

                if (!isRequestJsonValid)
                {
                    // todo: log these errors to telemetry event
                    var result = new BadRequestObjectResult(redactions.Errors.Select(e => new
                    {
                        Field = e.PropertyName,
                        Error = e.ErrorMessage
                    }));

                    return sendTelemetryAndReturn(result);
                }

                var redactPdfRequest = _redactPdfRequestMapper.Map(redactions.Value);
                await _pipelineClient.SaveRedactionsAsync(caseUrn, caseId, new PolarisDocumentId(polarisDocumentId), redactPdfRequest, CmsAuthValues, CorrelationId);
                telemetryEvent.IsSuccess = true;

                return sendTelemetryAndReturn(new OkResult());
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackEventFailure(telemetryEvent);
                return HandleUnhandledException(exception);
            }
        }
    }
}

