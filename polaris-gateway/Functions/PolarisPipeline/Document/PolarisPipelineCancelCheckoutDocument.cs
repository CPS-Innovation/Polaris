using Common.Configuration;
using Common.Logging;
using PolarisGateway.Domain.Validators.Contracts;
using Common.ValueObjects;
using Gateway.Clients.PolarisPipeline.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Telemetry.Wrappers.Contracts;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PolarisGateway.Functions.PolarisPipeline.Document
{
    public class PolarisPipelineCancelCheckoutDocument : BasePolarisFunction
    {
        private readonly IPipelineClient _pipelineClient;
        private readonly ILogger<PolarisPipelineCancelCheckoutDocument> _logger;

        const string loggingName = $"{nameof(PolarisPipelineCancelCheckoutDocument)} - {nameof(Run)}";

        public PolarisPipelineCancelCheckoutDocument
            (
                IPipelineClient pipelineClient,
                ILogger<PolarisPipelineCancelCheckoutDocument> logger,
                IAuthorizationValidator tokenValidator,
                ITelemetryAugmentationWrapper telemetryAugmentationWrapper
            )
        : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _pipelineClient = pipelineClient;
            _logger = logger;
        }

        [FunctionName(nameof(PolarisPipelineCancelCheckoutDocument))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = RestApi.DocumentCheckout)] HttpRequest req, string caseUrn, int caseId, string polarisDocumentId)
        {
            Guid currentCorrelationId = default;

            try
            {
                #region Validate-Inputs
                var request = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (request.InvalidResponseResult != null)
                    return request.InvalidResponseResult;

                currentCorrelationId = request.CurrentCorrelationId;
                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                if (string.IsNullOrWhiteSpace(caseUrn))
                    return BadRequestErrorResponse("Urn is not supplied.", currentCorrelationId, loggingName);
                #endregion

                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Cancel checkout of document with urn {caseUrn}, caseId {caseId}, polarisDocumentId {polarisDocumentId}");
                await _pipelineClient.CancelCheckoutDocumentAsync(caseUrn, caseId, new PolarisDocumentId(polarisDocumentId), request.CmsAuthValues, currentCorrelationId);

                return new OkResult();
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    HttpRequestException => InternalServerErrorResponse(exception, $"A pipeline client http exception occurred when calling {nameof(_pipelineClient.CancelCheckoutDocumentAsync)}.", currentCorrelationId, loggingName),
                    _ => InternalServerErrorResponse(exception, "An unhandled exception occurred.", currentCorrelationId, loggingName)
                };
            }
            finally
            {
                _logger.LogMethodExit(currentCorrelationId, loggingName, string.Empty);
            }
        }
    }
}

