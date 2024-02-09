using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Common.Configuration;
using PolarisGateway.Domain.Validators;
using Gateway.Clients;
using Common.Telemetry.Wrappers.Contracts;

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

        [FunctionName(nameof(PolarisPipelineGetCaseTracker))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseTracker)] HttpRequest req, string caseUrn, int caseId)
        {
            Guid currentCorrelationId = default;


            try
            {
                var request = await ValidateRequest(req, nameof(PolarisPipelineGetCaseTracker), ValidRoles.UserImpersonation);
                if (request.InvalidResponseResult != null)
                    return request.InvalidResponseResult;

                currentCorrelationId = request.CurrentCorrelationId;

                if (string.IsNullOrWhiteSpace(caseUrn))
                    return BadRequestErrorResponse("A case URN was expected", currentCorrelationId, nameof(PolarisPipelineGetCaseTracker));

                var tracker = await _pipelineClient.GetTrackerAsync(caseUrn, caseId, currentCorrelationId);

                return tracker == null ? NotFoundErrorResponse($"No tracker found for case Urn '{caseUrn}', case id '{caseId}'.", currentCorrelationId, nameof(PolarisPipelineGetCaseTracker)) : new OkObjectResult(tracker);
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    MsalException => InternalServerErrorResponse(exception, "An onBehalfOfToken exception occurred.", currentCorrelationId, nameof(PolarisPipelineGetCaseTracker)),
                    HttpRequestException => InternalServerErrorResponse(exception, $"A pipeline client http exception occurred when calling {nameof(_pipelineClient.GetTrackerAsync)}.", currentCorrelationId, nameof(PolarisPipelineGetCaseTracker)),
                    _ => InternalServerErrorResponse(exception, "An unhandled exception occurred.", currentCorrelationId, nameof(PolarisPipelineGetCaseTracker))
                };
            }
        }
    }
}

