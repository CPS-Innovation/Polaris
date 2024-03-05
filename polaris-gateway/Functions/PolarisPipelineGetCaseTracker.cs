using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Configuration;
using PolarisGateway.Domain.Validators;
using PolarisGateway.Clients;
using Common.Telemetry.Wrappers.Contracts;
using System.Net;

namespace PolarisGateway.Functions
{
    public class PolarisPipelineGetCaseTracker : BasePolarisFunction
    {
        private readonly IPipelineClient _pipelineClient;

        public PolarisPipelineGetCaseTracker(ILogger<PolarisPipelineGetCaseTracker> logger,
                                         IPipelineClient pipelineClient,
                                         IAuthorizationValidator tokenValidator,
                                         ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _pipelineClient = pipelineClient;
        }

        [FunctionName(nameof(PolarisPipelineGetCaseTracker))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseTracker)] HttpRequest req, string caseUrn, int caseId)
        {
            try
            {
                await Initiate(req);

                var result = await _pipelineClient.GetTrackerAsync(caseUrn, caseId, CorrelationId);
                return new OkObjectResult(result);
            }
            catch (Exception exception)
            {
                if (exception is HttpRequestException h && h.StatusCode == HttpStatusCode.NotFound)
                {
                    return new NotFoundObjectResult($"No tracker found for case Urn '{caseUrn}', case id '{caseId}'.");
                }
                return HandleUnhandledException(exception);
            }
        }
    }
}

