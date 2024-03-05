using Common.Configuration;
using PolarisGateway.Domain.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Telemetry.Wrappers.Contracts;
using Gateway.Clients;

namespace PolarisGateway.Functions
{
    public class Cases : BasePolarisFunction
    {
        private readonly IPipelineClient _pipelineClient;

        public Cases(ILogger<Cases> logger,
                        IPipelineClient pipelineClient,
                        IAuthorizationValidator tokenValidator,
                        ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _pipelineClient = pipelineClient;
        }

        [FunctionName(nameof(Cases))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Cases)] HttpRequest req, string caseUrn)
        {
            try
            {
                await Initiate(req);

                var result = await _pipelineClient.GetCasesAsync(caseUrn, CmsAuthValues, CorrelationId);
                return new OkObjectResult(result);
            }
            catch (Exception exception)
            {
                return HandleUnhandledException(exception);
            }
        }
    }
}

