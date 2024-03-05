using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using PolarisGateway.Domain.Validators;
using PolarisGateway.Clients;
using Common.Telemetry.Wrappers.Contracts;
using PolarisGateway.Factories;

namespace PolarisGateway.Functions
{
    public class PolarisPipelineCaseDelete : BasePolarisFunction
    {
        private readonly IPipelineClient _pipelineClient;
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
        }

        [FunctionName(nameof(PolarisPipelineCaseDelete))]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = RestApi.Case)] HttpRequest req, string caseUrn, int caseId)
        {
            try
            {
                await Initiate(req);

                await _pipelineClient.DeleteCaseAsync(caseUrn, caseId, CmsAuthValues, CorrelationId);
                return new AcceptedResult();
            }
            catch (Exception exception)
            {
                return HandleUnhandledException(exception);
            }
        }
    }
}

