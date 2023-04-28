using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Threading.Tasks;
using Common.Extensions;
using Common.Validators.Contracts;
using Microsoft.AspNetCore.Http;
using PolarisGateway.Wrappers;

namespace PolarisGateway.Functions.Health
{
    public class Status : BasePolarisFunction
    {
        public Status(ILogger<Status> logger, IAuthorizationValidator authorizationValidator, ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
            : base(logger, authorizationValidator, telemetryAugmentationWrapper)
        { }

        [FunctionName("Status")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status")] HttpRequest req)
        {
            const string loggingName = "Status - Run";

            var validationResult = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
            if (validationResult.InvalidResponseResult != null)
                return validationResult.InvalidResponseResult;

            var response = Assembly.GetExecutingAssembly().CurrentStatus();
            var result = response == null ? new JsonResult(new { status = "Assembly version could not be retrieved" }) {StatusCode = (int)HttpStatusCode.BadRequest} 
                : new JsonResult(response) {StatusCode = (int)HttpStatusCode.OK};

            return result;
        }
    }
}

