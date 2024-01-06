using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using Common.Configuration;
using Common.Validators.Contracts;
using Gateway.Clients.PolarisPipeline.Contracts;
using PolarisGateway.Domain.Validators;
using Common.Mappers.Contracts;
using Gateway.Common.Extensions;
using Common.Telemetry.Wrappers.Contracts;
using Common.Dto.Request;
using Common.ValueObjects;
using Common.Telemetry.Contracts;
using PolarisGateway.TelemetryEvents;
using Common.Domain.Validation;
using System.Linq;

namespace PolarisGateway.Functions.PolarisPipeline.Document
{
    public class PolarisPipelineSaveDocumentRedactions : BasePolarisFunction
    {
        private readonly IRedactPdfRequestMapper _redactPdfRequestMapper;
        private readonly IPipelineClient _pipelineClient;
        private readonly ILogger<PolarisPipelineSaveDocumentRedactions> _logger;
        private readonly ITelemetryClient _telemetryClient;
        const string loggingName = $"{nameof(PolarisPipelineSaveDocumentRedactions)} - {nameof(Run)}";

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
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.Document)] HttpRequest req, string caseUrn, int caseId, string polarisDocumentId)
        {
            Guid currentCorrelationId = default;
            var telemetryEvent = new RedactionRequestEvent(caseId, polarisDocumentId);

            IActionResult sendTelemetryAndReturn(IActionResult result)
            {
                _telemetryClient.TrackEvent(telemetryEvent);
                return result;
            }

            try
            {
                var request = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                var isRequestValid = request.InvalidResponseResult == null;
                telemetryEvent.IsRequestValid = isRequestValid;
                currentCorrelationId = telemetryEvent.CorrelationId = request.CurrentCorrelationId;

                if (!isRequestValid)
                {
                    return sendTelemetryAndReturn(request.InvalidResponseResult);
                }

                var redactions = await req.GetJsonBody<DocumentRedactionSaveRequestDto, DocumentRedactionSaveRequestValidator>();
                var isRequestJsonValid = redactions.IsValid;
                telemetryEvent.IsRequestJsonValid = isRequestJsonValid;
                telemetryEvent.RequestJson = redactions.RequestJson;

                if (!isRequestJsonValid)
                {
                    return sendTelemetryAndReturn(ToBadRequest(redactions));
                }

                var polarisDocumentIdValue = new PolarisDocumentId(polarisDocumentId);
                var redactPdfRequest = _redactPdfRequestMapper.Map(redactions.Value, caseId, polarisDocumentIdValue, currentCorrelationId);
                var redactionResult = await _pipelineClient.SaveRedactionsAsync(caseUrn, caseId, polarisDocumentIdValue, redactPdfRequest, request.CmsAuthValues, currentCorrelationId);
                var IsSuccess = redactionResult.Succeeded;
                telemetryEvent.IsSuccess = IsSuccess;

                var result = IsSuccess
                    ? new OkResult()
                    : BadGatewayErrorResponse("Error Saving redaction details", currentCorrelationId, loggingName);

                return sendTelemetryAndReturn(result);
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackEventFailure(telemetryEvent);
                return exception switch
                {
                    HttpRequestException => InternalServerErrorResponse(exception, $"A pipeline client http exception occurred when calling {nameof(_pipelineClient.SaveRedactionsAsync)}, '{exception.Message}'.", currentCorrelationId, loggingName),
                    _ => InternalServerErrorResponse(exception, $"An unhandled exception occurred, '{exception.Message}'.", currentCorrelationId, loggingName)
                };
            }
        }

        private static BadRequestObjectResult ToBadRequest<T>(ValidatableRequest<T> request)
        {
            return new BadRequestObjectResult(request.Errors.Select(e => new
            {
                Field = e.PropertyName,
                Error = e.ErrorMessage
            }));
        }
    }
}

