using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Threading.Tasks;
using PolarisGateway.Domain.Logging;
using PolarisGateway.Domain.Validators;
using PolarisGateway.Extensions;
using PolarisGateway.Wrappers;

namespace PolarisGateway.Functions.Status
{
    public class Status : BasePolarisFunction
    {
        private readonly ILogger<Status> _logger;

        public Status(ILogger<Status> logger, IAuthorizationValidator authorizationValidator, ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
            : base(logger, authorizationValidator, telemetryAugmentationWrapper)
        {
            _logger = logger;
        }

        [FunctionName("Status")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status")] HttpRequest req)
        {
            const string loggingName = "Status - Run";

            var validationResult = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
            if (validationResult.InvalidResponseResult != null)
                return validationResult.InvalidResponseResult;

            var currentCorrelationId = validationResult.CurrentCorrelationId;
            _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

            var version = Assembly
                .GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;

            var response = new Domain.Status.Status
            {
                Message = $"Gateway - Version : {version}"
            };

            _logger.LogMethodExit(currentCorrelationId, loggingName, response.ToJson());
            return new OkObjectResult(response);
        }
    }
}

