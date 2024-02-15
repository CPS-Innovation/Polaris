using System.Reflection;
using Common.Configuration;
using Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace PolarisGateway.Functions;

public static class Status
{
    [FunctionName("Status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Status)] HttpRequest req)
    {
        return Assembly.GetExecutingAssembly().CurrentStatus();
    }
}