using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using PolarisGateway.Extensions;
using Common.Configuration;
using Common.Logging;
using PolarisGateway.Domain.Validators.Contracts;
using Gateway.Clients.PolarisPipeline.Contracts;
using Common.Telemetry.Wrappers.Contracts;
using Common.Dto.Tracker;
using PolarisGateway.Domain.PolarisPipeline;
using PolarisGateway.Factories.Contracts;
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

        const string loggingName = $"{nameof(PolarisPipelineCase)} - {nameof(Run)}";

        [FunctionName(nameof(PolarisPipelineCase))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", "delete", Route = RestApi.Case)] HttpRequest req, string caseUrn, int caseId)
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

                switch (req.Method.ToUpperInvariant())
                {
                    case "GET":
                        return new OkResult();

                    case "POST":
                        var response = await _pipelineClient.RefreshCaseAsync(caseUrn, caseId, request.CmsAuthValues, currentCorrelationId);
                        TriggerCoordinatorResponse trackerUrlResponse = _triggerCoordinatorResponseFactory.Create(req, currentCorrelationId);
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
                    MsalException => InternalServerErrorResponse(exception, "An onBehalfOfToken exception occurred.", currentCorrelationId, loggingName),
                    HttpRequestException => InternalServerErrorResponse(exception, $"A pipeline client http exception occurred when calling {nameof(_pipelineClient.DeleteCaseAsync)}.", currentCorrelationId, loggingName),
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

