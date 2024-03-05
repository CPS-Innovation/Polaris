using Common.Configuration;
using PolarisGateway.Domain.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Common.Telemetry.Wrappers.Contracts;
using PolarisGateway.Clients;

namespace PolarisGateway.Functions
{
    public class Cases : BasePolarisFunction
    {
        private readonly ICoordinatorClient _coordinatorClient;

        public Cases(ILogger<Cases> logger,
                        ICoordinatorClient coordinatorClient,
                        IAuthorizationValidator tokenValidator,
                        ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
        : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _coordinatorClient = coordinatorClient;
        }

        [FunctionName(nameof(Cases))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Cases)] HttpRequest req, string caseUrn)
        {
            try
            {
                await Initiate(req);
                return await _coordinatorClient.GetCasesAsync(caseUrn, CmsAuthValues, CorrelationId);
            }
            catch (Exception exception)
            {
                return HandleUnhandledExceptionHttpResponseMessage(exception);
            }
        }
    }
}

