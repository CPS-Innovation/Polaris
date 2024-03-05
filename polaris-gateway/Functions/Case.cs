using Common.Configuration;
using PolarisGateway.Domain.Validators;
using Ddei.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Telemetry.Wrappers.Contracts;
using Gateway.Clients;

namespace PolarisGateway.Functions
{
    public class Case : BasePolarisFunction
    {
        private readonly IPipelineClient _pipelineClient;

        public Case(ILogger<Case> logger,
                    IPipelineClient pipelineClient,
                    IAuthorizationValidator tokenValidator,
                    ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
            : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _pipelineClient = pipelineClient;
        }

        [FunctionName(nameof(Case))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Case)] HttpRequest req, string caseUrn, int caseId)
        {
            try
            {
                await Initiate(req);

                var result = await _pipelineClient.GetCaseAsync(caseUrn, caseId, CmsAuthValues, CorrelationId);
                return new OkObjectResult(result);
            }
            catch (Exception exception)
            {
                return HandleUnhandledException(exception);
            }
        }
    }
}

