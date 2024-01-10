using System;
using System.Net;
using polaris_common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;

namespace pdf_generator.Functions;

public class GetHostName
{
    private readonly ILogger _logger;

    public GetHostName(ILogger<GetHostName> logger)
    {
        _logger = logger;
    }

    [Function("GetHostName")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.GetHostName)] HttpRequest req)
    {
        return new JsonResult(Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME")) { StatusCode = (int)HttpStatusCode.OK };
    }
}