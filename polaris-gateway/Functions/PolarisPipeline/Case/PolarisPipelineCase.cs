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
using PolarisGateway.Factories;
using Common.Domain.Exceptions;

namespace PolarisGateway.Functions.PolarisPipeline.Case
{
    public class PolarisPipelineCase : BasePolarisFunction
    {
        private readonly IPipelineClient _pipelineClient;
        private readonly ILogger<PolarisPipelineCase> _logger;
        private readonly ITriggerCoordinatorResponseFactory _triggerCoordinatorResponseFactory;

        public PolarisPipelineCase(ILogger<PolarisPipelineCase> logger,
                                    IPipelineClient pipelineClient,
                                    IAuthorizationValidator tokenValidator,
                                    ITriggerCoordinatorResponseFactory triggerCoordinatorResponseFactory,
                                    ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _pipelineClient = pipelineClient;
            _triggerCoordinatorResponseFactory = triggerCoordinatorResponseFactory;
            _logger = logger;
        }

        [FunctionName(nameof(PolarisPipelineCase))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", "delete", Route = RestApi.Case)] HttpRequest req, string caseUrn, int caseId)
        {
            Guid currentCorrelationId = default;

            try
            {
                var request = await ValidateRequest(req, nameof(PolarisPipelineCase), ValidRoles.UserImpersonation);
                if (request.InvalidResponseResult != null)
                    return request.InvalidResponseResult;

                currentCorrelationId = request.CurrentCorrelationId;

                if (string.IsNullOrWhiteSpace(caseUrn))
                    return BadRequestErrorResponse("A case URN was expected", currentCorrelationId, nameof(PolarisPipelineCase));

                switch (req.Method.ToUpperInvariant())
                {
                    case "POST":
                        var response = await _pipelineClient.RefreshCaseAsync(caseUrn, caseId, request.CmsAuthValues, currentCorrelationId);
                        var trackerUrlResponse = _triggerCoordinatorResponseFactory.Create(req, currentCorrelationId);
                        return new ObjectResult(trackerUrlResponse)
                        {
                            StatusCode = response.StatusCode
                        };

                    case "DELETE":
                        await _pipelineClient.DeleteCaseAsync(caseUrn, caseId, request.CmsAuthValues, currentCorrelationId);
                        return new OkResult();

                    default:
                        throw new BadRequestException("Unexpected HTTP Verb", req.Method);
                }
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    MsalException => InternalServerErrorResponse(exception, "An onBehalfOfToken exception occurred.", currentCorrelationId, nameof(PolarisPipelineCase)),
                    HttpRequestException => InternalServerErrorResponse(exception, $"A pipeline client http exception occurred when calling {nameof(_pipelineClient.DeleteCaseAsync)}.", currentCorrelationId, nameof(PolarisPipelineCase)),
                    _ => InternalServerErrorResponse(exception, "An unhandled exception occurred.", currentCorrelationId, nameof(PolarisPipelineCase))
                };
            }
        }
    }
}

