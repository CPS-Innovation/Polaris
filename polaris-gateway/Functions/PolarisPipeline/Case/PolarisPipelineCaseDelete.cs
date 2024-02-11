using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using PolarisGateway.Domain.Validators;
using Gateway.Clients;
using Common.Telemetry.Wrappers.Contracts;
using PolarisGateway.Factories;

namespace PolarisGateway.Functions.PolarisPipeline.Case
{
    public class PolarisPipelineCaseDelete : BasePolarisFunction
    {
        private readonly IPipelineClient _pipelineClient;
        private readonly ILogger<PolarisPipelineCase> _logger;
        private readonly ITrackerResponseFactory _triggerCoordinatorResponseFactory;

        public PolarisPipelineCaseDelete(ILogger<PolarisPipelineCase> logger,
                                    IPipelineClient pipelineClient,
                                    IAuthorizationValidator tokenValidator,
                                    ITrackerResponseFactory triggerCoordinatorResponseFactory,
                                    ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _pipelineClient = pipelineClient;
            _triggerCoordinatorResponseFactory = triggerCoordinatorResponseFactory;
            _logger = logger;
        }

        [FunctionName(nameof(PolarisPipelineCaseDelete))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = RestApi.Case)] HttpRequest req, string caseUrn, int caseId)
        {
            try
            {
                await Initiate(req);

                return await _pipelineClient.DeleteCaseAsync(caseUrn, caseId, CmsAuthValues, CorrelationId);
            }
            catch (Exception exception)
            {
                return HandleUnhandledException(exception);
            }
        }
    }
}

