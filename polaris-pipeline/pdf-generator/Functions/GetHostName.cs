using System;
using System.Net;
using Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace pdf_generator.Functions;

public static class GetHostName
{
    [FunctionName("GetHostName")]
    public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.GetHostName)] HttpRequest req)
    {
        return new JsonResult(Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME")) {StatusCode = (int) HttpStatusCode.OK};
    }
}