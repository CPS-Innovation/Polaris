using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using RumpoleGateway.Clients.OnBehalfOfTokenClient;
using RumpoleGateway.Clients.RumpolePipeline;
using RumpoleGateway.Helpers.Extension;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using RumpoleGateway.Domain.Logging;
using RumpoleGateway.Domain.RumpolePipeline;
using RumpoleGateway.Domain.Validators;
using RumpoleGateway.Extensions;
using System.Net;

namespace RumpoleGateway.Functions.RumpolePipeline
{
    public class RumpolePipelineGetTracker : BaseRumpoleFunction
    {
        private readonly IOnBehalfOfTokenClient _onBehalfOfTokenClient;
        private readonly IPipelineClient _pipelineClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RumpolePipelineGetTracker> _logger;

        public RumpolePipelineGetTracker(ILogger<RumpolePipelineGetTracker> logger, IOnBehalfOfTokenClient onBehalfOfTokenClient, IPipelineClient pipelineClient,
                                 IConfiguration configuration, IAuthorizationValidator tokenValidator)
        : base(logger, tokenValidator)
        {
            _onBehalfOfTokenClient = onBehalfOfTokenClient;
            _pipelineClient = pipelineClient;
            _configuration = configuration;
            _logger = logger;
        }

        [FunctionName("RumpolePipelineGetTracker")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "urns/{urn}/cases/{caseId}/tracker")] HttpRequest req, string urn, int caseId)
        {
            Guid currentCorrelationId = default;
            const string loggingName = "RumpolePipelineGetTracker - Run";
            Tracker tracker = null;

            try
            {
                urn = WebUtility.UrlDecode(urn); // todo: inject or move to validator
                var validationResult = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (validationResult.InvalidResponseResult != null)
                    return validationResult.InvalidResponseResult;

                currentCorrelationId = validationResult.CurrentCorrelationId;
                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                if (string.IsNullOrWhiteSpace(urn))
                    return BadRequestErrorResponse("A case URN was expected", currentCorrelationId, loggingName);

                // if (!int.TryParse(caseId, out _))
                //     return BadRequestErrorResponse("Invalid case id. A 32-bit integer is required.", currentCorrelationId, loggingName);

                var coordinatorScope = _configuration[ConfigurationKeys.PipelineCoordinatorScope];
                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Getting an access token as part of OBO for the following scope {coordinatorScope}");
                var onBehalfOfAccessToken = await _onBehalfOfTokenClient.GetAccessTokenAsync(validationResult.AccessTokenValue.ToJwtString(), coordinatorScope, currentCorrelationId);

                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Getting tracker details for caseId {caseId}");
                tracker = await _pipelineClient.GetTrackerAsync(urn, caseId, onBehalfOfAccessToken, currentCorrelationId);

                return tracker == null ? NotFoundErrorResponse($"No tracker found for case Urn '{urn}', case id '{caseId}'.", currentCorrelationId, loggingName) : new OkObjectResult(tracker);
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    MsalException => InternalServerErrorResponse(exception, "An onBehalfOfToken exception occurred.", currentCorrelationId, loggingName),
                    HttpRequestException => InternalServerErrorResponse(exception, "A pipeline client http exception occurred when calling GetTracker.", currentCorrelationId,
                        loggingName),
                    _ => InternalServerErrorResponse(exception, "An unhandled exception occurred.", currentCorrelationId, loggingName)
                };
            }
            finally
            {
                _logger.LogMethodExit(currentCorrelationId, loggingName, tracker.ToJson());
            }
        }
    }
}

