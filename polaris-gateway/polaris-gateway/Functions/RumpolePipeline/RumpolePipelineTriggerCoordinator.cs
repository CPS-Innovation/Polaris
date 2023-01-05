using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using PolarisGateway.Clients.OnBehalfOfTokenClient;
using PolarisGateway.Clients.PolarisPipeline;
using PolarisGateway.Factories;
using PolarisGateway.Helpers.Extension;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using PolarisGateway.Domain.Logging;
using PolarisGateway.Domain.Validators;
using System.Net;

namespace PolarisGateway.Functions.PolarisPipeline
{
    public class PolarisPipelineTriggerCoordinator : BasePolarisFunction
    {
        private readonly IOnBehalfOfTokenClient _onBehalfOfTokenClient;
        private readonly IPipelineClient _pipelineClient;
        private readonly IConfiguration _configuration;
        private readonly ITriggerCoordinatorResponseFactory _triggerCoordinatorResponseFactory;
        private readonly ILogger<PolarisPipelineTriggerCoordinator> _logger;

        public PolarisPipelineTriggerCoordinator(ILogger<PolarisPipelineTriggerCoordinator> logger, IOnBehalfOfTokenClient onBehalfOfTokenClient,
                                 IPipelineClient pipelineClient, IConfiguration configuration,
                                 ITriggerCoordinatorResponseFactory triggerCoordinatorResponseFactory, IAuthorizationValidator tokenValidator)
        : base(logger, tokenValidator)
        {
            _onBehalfOfTokenClient = onBehalfOfTokenClient;
            _pipelineClient = pipelineClient;
            _configuration = configuration;
            _triggerCoordinatorResponseFactory = triggerCoordinatorResponseFactory;
            _logger = logger;
        }

        [FunctionName("PolarisPipelineTriggerCoordinator")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "urns/{urn}/cases/{caseId}")] HttpRequest req, string urn, int caseId)
        {
            Guid currentCorrelationId = default;
            const string loggingName = "PolarisPipelineTriggerCoordinator - Run";

            try
            {
                urn = WebUtility.UrlDecode(urn); // todo: inject or move to validator
                var validationResult = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (validationResult.InvalidResponseResult != null)
                    return validationResult.InvalidResponseResult;

                currentCorrelationId = validationResult.CurrentCorrelationId;
                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                if (string.IsNullOrWhiteSpace(urn))
                    return BadRequestErrorResponse("An empty case URN was received - please correct.", currentCorrelationId, loggingName);

                // if (!int.TryParse(caseId, out var _))
                //     return BadRequestErrorResponse("Invalid case id. A 32-bit integer is required.", currentCorrelationId, loggingName);

                var force = false;
                if (req.Query.ContainsKey("force") && !bool.TryParse(req.Query["force"], out force))
                    return BadRequestErrorResponse("Invalid query string. Force value must be a boolean.", currentCorrelationId, loggingName);

                var scopes = _configuration[ConfigurationKeys.PipelineCoordinatorScope];
                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Getting an access token as part of OBO for the following scope {scopes}");
                var onBehalfOfAccessToken = await _onBehalfOfTokenClient.GetAccessTokenAsync(validationResult.AccessTokenValue.ToJwtString(), scopes, currentCorrelationId);

                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Triggering the pipeline for caseId: {caseId}, forceRefresh: {force}");
                await _pipelineClient.TriggerCoordinatorAsync(urn, caseId, onBehalfOfAccessToken, "sample-token", force, currentCorrelationId);

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

