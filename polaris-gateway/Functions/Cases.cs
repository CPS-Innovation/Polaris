using Common.Configuration;
using PolarisGateway.Domain.Validators;
using Ddei.Factories.Contracts;
using DdeiClient.Services.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Telemetry.Wrappers.Contracts;

namespace PolarisGateway.Functions
{
    public class Cases : BasePolarisFunction
    {
        private readonly IDdeiClient _ddeiClient;
        private readonly ICaseDataArgFactory _caseDataArgFactory;

        public Cases(ILogger<Cases> logger,
                        IDdeiClient caseDataService,
                        IAuthorizationValidator tokenValidator,
                        ICaseDataArgFactory caseDataArgFactory,
                        ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _ddeiClient = caseDataService;
            _caseDataArgFactory = caseDataArgFactory;
        }

        [FunctionName(nameof(Cases))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Cases)] HttpRequest req, string caseUrn)
        {
            try
            {
                await Initiate(req);

                var arg = _caseDataArgFactory.CreateUrnArg(CmsAuthValues, CorrelationId, caseUrn);
                var result = await _ddeiClient.ListCasesAsync(arg);

                return new OkObjectResult(result);
            }
            catch (Exception exception)
            {
                return HandleUnhandledException(exception);
            }
        }
    }
}

