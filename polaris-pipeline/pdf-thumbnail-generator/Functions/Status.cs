using System.Reflection;
using Common.Configuration;
using Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace pdf_thumbnail_generator.Functions
{ 
    public static class Status
    {
        [Function("Status")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Status)] HttpRequest req)
        {
            return Assembly.GetExecutingAssembly().CurrentStatus();
        }
    }
}
