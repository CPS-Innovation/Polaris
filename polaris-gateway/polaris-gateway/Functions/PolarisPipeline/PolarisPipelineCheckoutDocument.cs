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
using PolarisGateway.Wrappers;

namespace PolarisGateway.Functions.PolarisPipeline
{
    public class PolarisPipelineCheckoutDocument : BasePolarisFunction
    {
        private readonly IPipelineClient _pipelineClient;
        private readonly ILogger<PolarisPipelineCheckoutDocument> _logger;

        const string loggingName = $"{nameof(PolarisPipelineCheckoutDocument)} - {nameof(Run)}";

        public PolarisPipelineCheckoutDocument
            (
                IPipelineClient pipelineClient, 
                ILogger<PolarisPipelineCheckoutDocument> logger, 
                IAuthorizationValidator tokenValidator,
                ITelemetryAugmentationWrapper telemetryAugmentationWrapper
            )
        : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _pipelineClient = pipelineClient;
            _logger = logger;
        }

        [FunctionName(nameof(PolarisPipelineCheckoutDocument))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.DocumentCheckout)] HttpRequest req, string caseUrn, int caseId, Guid documentId)
        {
            Guid currentCorrelationId = default;

            try
            {
                var request = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (request.InvalidResponseResult != null)
                    return request.InvalidResponseResult;

                currentCorrelationId = request.CurrentCorrelationId;
                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                if (string.IsNullOrWhiteSpace(caseUrn))
                    return BadRequestErrorResponse("Urn is not supplied.", currentCorrelationId, loggingName);

                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Checking out document for urn {caseUrn}, caseId {caseId}, id {documentId}");
                await _pipelineClient.CheckoutDocumentAsync(caseUrn, caseId, documentId, request.CmsAuthValues, currentCorrelationId);

                return new OkResult();
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    HttpRequestException => InternalServerErrorResponse(exception, $"A pipeline client http exception occurred when calling {nameof(_pipelineClient.CheckoutDocumentAsync)}.", currentCorrelationId, loggingName),
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

