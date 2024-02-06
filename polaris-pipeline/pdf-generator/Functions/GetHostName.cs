using System;
using System.Net;
using Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace pdf_generator.Functions;

public class GetHostName
{
    [Function("GetHostName")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.GetHostName)] HttpRequest req)
    {
        return new JsonResult(Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME")) { StatusCode = (int)HttpStatusCode.OK };
    }
}