using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using PolarisGateway.Factories.Contracts;
using PolarisGateway.Wrappers;
using Common.Configuration;
using Common.Logging;
using Common.Validators.Contracts;
using Gateway.Clients.PolarisPipeline.Contracts;

namespace PolarisGateway.Functions.PolarisPipeline.Case
{
    public class PolarisPipelineRefreshCase : BasePolarisFunction
    {
        private readonly IPipelineClient _pipelineClient;
        private readonly ITriggerCoordinatorResponseFactory _triggerCoordinatorResponseFactory;
        private readonly ILogger<PolarisPipelineRefreshCase> _logger;

        const string loggingName = $"{nameof(PolarisPipelineRefreshCase)} - {nameof(Run)}";

        public PolarisPipelineRefreshCase(ILogger<PolarisPipelineRefreshCase> logger,
                                                 IPipelineClient pipelineClient,
                                                 ITriggerCoordinatorResponseFactory triggerCoordinatorResponseFactory,
                                                 IAuthorizationValidator tokenValidator,
                                                 ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _pipelineClient = pipelineClient;
            _triggerCoordinatorResponseFactory = triggerCoordinatorResponseFactory;
            _logger = logger;
        }

        [FunctionName(nameof(PolarisPipelineRefreshCase))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.Case)] HttpRequest req, string caseUrn, int caseId)
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
                    return BadRequestErrorResponse("An empty case URN was received", currentCorrelationId, loggingName);

                await _pipelineClient.RefreshCaseAsync(caseUrn, caseId, request.CmsAuthValues, currentCorrelationId);

                return new OkObjectResult(_triggerCoordinatorResponseFactory.Create(req, currentCorrelationId));
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    MsalException => InternalServerErrorResponse(exception, "An onBehalfOfToken exception occurred.", currentCorrelationId, loggingName),
                    HttpRequestException => InternalServerErrorResponse(exception, "A pipeline client http exception occurred.", currentCorrelationId, loggingName),
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

