using System.Net;
using System.Reflection;
using Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace PolarisAuthHandover.Functions.Health;

public static class Status
{
    [FunctionName("Status")]
    public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "status")] HttpRequest req)
    {
        var response = Assembly.GetExecutingAssembly().CurrentStatus();
        var result = response == null ? new JsonResult(new { status = "Assembly version could not be retrieved" }) {StatusCode = (int)HttpStatusCode.BadRequest} 
            : new JsonResult(response) {StatusCode = (int)HttpStatusCode.OK};

        return result;
    }
}
