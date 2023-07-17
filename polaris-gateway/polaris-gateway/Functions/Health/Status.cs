using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Reflection;
using Common.Extensions;
using Microsoft.AspNetCore.Http;
using Common.Configuration;

namespace PolarisGateway.Functions.Health
{
    public class Status
    {
        [FunctionName("Status")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Status)] HttpRequest req)
        {
            return Assembly.GetExecutingAssembly().CurrentStatus();
        }
    }
}

