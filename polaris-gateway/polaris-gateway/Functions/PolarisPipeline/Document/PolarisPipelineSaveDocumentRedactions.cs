using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using Common.Configuration;
using Common.Logging;
using Common.Validators.Contracts;
using Gateway.Clients.PolarisPipeline.Contracts;
using PolarisGateway.Domain.Validators;
using PolarisGateway.Extensions;
using Common.Mappers.Contracts;
using Gateway.Common.Extensions;
using Common.Telemetry.Wrappers.Contracts;
using Common.Dto.Request;
using Common.ValueObjects;
using Common.Telemetry.Contracts;
using PolarisGateway.TelemetryEvents;

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
            _telemetryClient = telemetryClient;
        }

        [FunctionName(nameof(PolarisPipelineSaveDocumentRedactions))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.Document)] HttpRequest req, string caseUrn, int caseId, string polarisDocumentId)
        {
            Guid currentCorrelationId = default;
            var telemetryEvent = new RedactionRequestReceivedEvent(caseId, polarisDocumentId);
            try
            {
                #region Validate-Inputs
                var request = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (request.InvalidResponseResult != null)
                    return request.InvalidResponseResult;

                currentCorrelationId = telemetryEvent.CorrelationId = request.CurrentCorrelationId;

                var redactions = await req.GetJsonBody<DocumentRedactionSaveRequestDto, DocumentRedactionSaveRequestValidator>();
                var isValid = redactions.IsValid;
                telemetryEvent.RequestJson = redactions.RequestJson;
                telemetryEvent.IsRequestJsonValid = isValid;
                _telemetryClient.TrackEvent(telemetryEvent);

                if (!isValid)
                {
                    LogInformation("Invalid redaction request", currentCorrelationId, loggingName);
                    return redactions.ToBadRequest();
                }
                #endregion

                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Saving document redactions for urn {caseUrn}, caseId {caseId}, polarisDocumentId {polarisDocumentId}");
                var polarisDocumentIdValue = new PolarisDocumentId(polarisDocumentId);
                var redactPdfRequest = _redactPdfRequestMapper.Map(redactions.Value, caseId, polarisDocumentIdValue, currentCorrelationId);
                var redactionResult = await _pipelineClient.SaveRedactionsAsync(caseUrn, caseId, polarisDocumentIdValue, redactPdfRequest, request.CmsAuthValues, currentCorrelationId);

                if (!redactionResult.Succeeded)
                {
                    _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Error Saving redaction details to the document for {caseId}, polarisDocumentId {polarisDocumentId}");
                    return BadGatewayErrorResponse("Error Saving redaction details", currentCorrelationId, loggingName);
                }

                return new OkResult();
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
            finally
            {
                _logger.LogMethodExit(currentCorrelationId, loggingName, string.Empty);
            }
        }
    }
}

