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
    public class Status
    {
        [FunctionName("Status")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status")] HttpRequest req)
        {
            return Assembly.GetExecutingAssembly().CurrentStatus();
        }
    }
}

