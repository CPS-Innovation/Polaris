using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using PolarisGateway.Domain.PolarisPipeline;
using PolarisGateway.Extensions;
using Common.Configuration;
using Common.Logging;
using Common.Validators.Contracts;
using Gateway.Clients.PolarisPipeline.Contracts;
using PolarisGateway.Wrappers;

namespace PolarisGateway.Functions.PolarisPipeline.Case
{
    public class PolarisPipelineGetCaseTracker : BasePolarisFunction
    {
        private readonly IPipelineClient _pipelineClient;
        private readonly ILogger<PolarisPipelineGetCaseTracker> _logger;

        public PolarisPipelineGetCaseTracker(ILogger<PolarisPipelineGetCaseTracker> logger,
                                         IPipelineClient pipelineClient,
                                         IAuthorizationValidator tokenValidator,
                                         ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _pipelineClient = pipelineClient;
            _logger = logger;
        }

        const string loggingName = $"{nameof(PolarisPipelineGetCaseTracker)} - {nameof(Run)}";

        [FunctionName(nameof(PolarisPipelineGetCaseTracker))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseTracker)] HttpRequest req, string caseUrn, int caseId)
        {
            Guid currentCorrelationId = default;
            TrackerDto tracker = null;

            try
            {
                var request = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (request.InvalidResponseResult != null)
                    return request.InvalidResponseResult;

                currentCorrelationId = request.CurrentCorrelationId;
                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                if (string.IsNullOrWhiteSpace(caseUrn))
                    return BadRequestErrorResponse("A case URN was expected", currentCorrelationId, loggingName);

                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Getting tracker details for caseId {caseId}");
                tracker = await _pipelineClient.GetTrackerAsync(caseUrn, caseId, currentCorrelationId);

                return tracker == null ? NotFoundErrorResponse($"No tracker found for case Urn '{caseUrn}', case id '{caseId}'.", currentCorrelationId, loggingName) : new OkObjectResult(tracker);
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    MsalException => InternalServerErrorResponse(exception, "An onBehalfOfToken exception occurred.", currentCorrelationId, loggingName),
                    HttpRequestException => InternalServerErrorResponse(exception, $"A pipeline client http exception occurred when calling {nameof(_pipelineClient.GetTrackerAsync)}.", currentCorrelationId, loggingName),
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

