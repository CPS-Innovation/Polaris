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

namespace PolarisGateway.Functions
{
    public class PolarisPipelineCheckoutDocument : BasePolarisFunction
    {
        private readonly IPipelineClient _pipelineClient;

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
        }

        [FunctionName(nameof(PolarisPipelineCheckoutDocument))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.DocumentCheckout)] HttpRequest req, string caseUrn, int caseId, string polarisDocumentId)
        {
            try
            {
                await Initiate(req);

                return await _pipelineClient.CheckoutDocumentAsync(caseUrn, caseId, new PolarisDocumentId(polarisDocumentId), CmsAuthValues, CorrelationId);
            }
            catch (Exception exception)
            {
                return HandleUnhandledException(exception);
            }
        }
    }
}

