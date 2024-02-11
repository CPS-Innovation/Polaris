using Common.Configuration;
using PolarisGateway.Domain.Validators;
using Ddei.Exceptions;
using Ddei.Factories.Contracts;
using DdeiClient.Services.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Telemetry.Wrappers.Contracts;

namespace PolarisGateway.Functions.CaseData
{
    public class Case : BasePolarisFunction
    {
        private readonly IDdeiClient _ddeiClient;
        private readonly ICaseDataArgFactory _caseDataArgFactory;

        public Case(ILogger<Case> logger,
                    IDdeiClient ddeiService,
                    IAuthorizationValidator tokenValidator,
                    ICaseDataArgFactory caseDataArgFactory,
                    ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
            : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _ddeiClient = ddeiService;
            _caseDataArgFactory = caseDataArgFactory;
        }

        [FunctionName(nameof(Case))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Case)] HttpRequest req, string caseUrn, int caseId)
        {
            try
            {
                await Initiate(req);

                var arg = _caseDataArgFactory.CreateCaseArg(CmsAuthValues, CorrelationId, caseUrn, caseId);
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

