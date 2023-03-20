using System;
using System.Net;
using Common.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PolarisDomain.Functions;

public class BaseFunction
{
    private readonly ILogger _logger;

    protected BaseFunction(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected IActionResult InternalServerErrorResponse(Exception exception, string additionalMessage, Guid correlationId, string loggerSource, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    {
        _logger.LogMethodError(correlationId, loggerSource, additionalMessage, exception);
        return new ObjectResult(additionalMessage) { StatusCode = (int)statusCode };
    }

    protected IActionResult BadRequestErrorResponse(string errorMessage, Guid correlationId, string loggerSource)
    {
        _logger.LogMethodFlow(correlationId, loggerSource, errorMessage);
        return new BadRequestObjectResult(errorMessage);
    }

    protected void LogInformation(string message, Guid correlationId, string loggerSource)
    {
        _logger.LogMethodFlow(correlationId, loggerSource, message);
    }
}
