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
    // note: the analytics KQL queries refer to "PolarisPipelineCase" as the function name,
    //  if we change this then we must change the KQL queries to be `| ... ("PolarisPipelineCase" or "NewName")
    public class PolarisPipelineCase : BasePolarisFunction
    {
        private readonly IPipelineClient _pipelineClient;
        private readonly ITrackerResponseFactory _triggerCoordinatorResponseFactory;

        public PolarisPipelineCase(ILogger<PolarisPipelineCase> logger,
                                    IPipelineClient pipelineClient,
                                    IAuthorizationValidator tokenValidator,
                                    ITrackerResponseFactory triggerCoordinatorResponseFactory,
                                    ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _pipelineClient = pipelineClient;
            _triggerCoordinatorResponseFactory = triggerCoordinatorResponseFactory;
        }

        [FunctionName(nameof(PolarisPipelineCase))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.Case)] HttpRequest req, string caseUrn, int caseId)
        {
            try
            {
                await Initiate(req);

                var responseCode = await _pipelineClient.RefreshCaseAsync(caseUrn, caseId, CmsAuthValues, CorrelationId);
                var result = _triggerCoordinatorResponseFactory.Create(req, CorrelationId);
                return new ObjectResult(result)
                {
                    StatusCode = (int)responseCode
                };
            }
            catch (Exception exception)
            {
                return HandleUnhandledException(exception);
            }
        }
    }
}

