using Common.Configuration;
using PolarisGateway.Domain.Validators;
using Ddei.Factories;
using DdeiClient.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Telemetry.Wrappers.Contracts;

namespace PolarisGateway.Functions
{
    public class Case : BasePolarisFunction
    {
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;

        public Case(ILogger<Case> logger,
                    IDdeiClient ddeiService,
                    IAuthorizationValidator tokenValidator,
                    IDdeiArgFactory ddeiArgFactory,
                    ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
            : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _ddeiClient = ddeiService;
            _ddeiArgFactory = ddeiArgFactory;
        }

        [FunctionName(nameof(Case))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Case)] HttpRequest req, string caseUrn, int caseId)
        {
            try
            {
                await Initiate(req);

                var arg = _ddeiArgFactory.CreateCaseArg(CmsAuthValues, CorrelationId, caseUrn, caseId);
                var result = await _ddeiClient.GetCaseAsync(arg);

                return new OkObjectResult(result);
            }
            catch (Exception exception)
            {
                return HandleUnhandledException(exception);
            }
        }
    }
}

