using Common.Configuration;
using PolarisGateway.Domain.Validators;
using Common.ValueObjects;
using Gateway.Clients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Telemetry.Wrappers.Contracts;

namespace PolarisGateway.Functions.PolarisPipeline.Document
{
    public class PolarisPipelineCancelCheckoutDocument : BasePolarisFunction
    {
        private readonly IPipelineClient _pipelineClient;
        private readonly ILogger<PolarisPipelineCancelCheckoutDocument> _logger;

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
            try
            {
                await Initiate(req);

                await _pipelineClient.CancelCheckoutDocumentAsync(caseUrn, caseId, new PolarisDocumentId(polarisDocumentId), CmsAuthValues, CorrelationId);
                return new OkResult();
            }
            catch (Exception exception)
            {
                return HandleUnhandledException(exception);
            }
        }
    }
}

