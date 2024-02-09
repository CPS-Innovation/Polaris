using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using PolarisGateway.Domain.Validators;
using Gateway.Clients;
using Common.Telemetry.Wrappers.Contracts;
using Common.ValueObjects;

namespace PolarisGateway.Functions.PolarisPipeline.Document
{
    public class PolarisPipelineCheckoutDocument : BasePolarisFunction
    {
        private readonly IPipelineClient _pipelineClient;
        private readonly ILogger<PolarisPipelineCheckoutDocument> _logger;

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
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.DocumentCheckout)] HttpRequest req, string caseUrn, int caseId, string polarisDocumentId)
        {
            Guid currentCorrelationId = default;

            try
            {
                var request = await ValidateRequest(req, nameof(PolarisPipelineCheckoutDocument), ValidRoles.UserImpersonation);
                if (request.InvalidResponseResult != null)
                    return request.InvalidResponseResult;

                currentCorrelationId = request.CurrentCorrelationId;

                if (string.IsNullOrWhiteSpace(caseUrn))
                    return BadRequestErrorResponse("Urn is not supplied.", currentCorrelationId, nameof(PolarisPipelineCheckoutDocument));

                var response = await _pipelineClient.CheckoutDocumentAsync(caseUrn, caseId, new PolarisDocumentId(polarisDocumentId), request.CmsAuthValues, currentCorrelationId);

                return response;
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    HttpRequestException => InternalServerErrorResponse(exception, $"A pipeline client http exception occurred when calling {nameof(_pipelineClient.CheckoutDocumentAsync)}.", currentCorrelationId, nameof(PolarisPipelineCheckoutDocument)),
                    _ => InternalServerErrorResponse(exception, "An unhandled exception occurred.", currentCorrelationId, nameof(PolarisPipelineCheckoutDocument))
                };
            }
        }
    }
}

