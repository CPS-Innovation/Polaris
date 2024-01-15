using System.Reflection;
using Common.Configuration;
using Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;

namespace pdf_generator.Functions
{
    public class Status
    {
        private readonly ILogger _logger;

        public Status(ILogger<Status> logger)
        {
            _logger = logger;
        }

        [Function("Status")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.Status)] HttpRequest req)
        {
            return Assembly.GetExecutingAssembly().CurrentStatus();
        }
    }
}
