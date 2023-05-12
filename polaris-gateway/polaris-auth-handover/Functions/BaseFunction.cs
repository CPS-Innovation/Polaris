using System;
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

    protected IActionResult HandleUnhandledException(Exception exception, Guid currentCorrelationId, string loggingName)
    {
        return InternalServerErrorResponse(
              NestedMessage(exception), currentCorrelationId, loggingName, exception
            );
    }

    protected IActionResult InternalServerErrorResponse(string errorMessage, Guid correlationId, string loggerSource, Exception exception)
    {
        _logger.LogMethodError(correlationId, loggerSource, errorMessage, exception);
        return new ObjectResult(errorMessage) { StatusCode = 500 };
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

    private string NestedMessage(Exception exception)
    {
        if (exception == null)
        {
            return string.Empty;
        }

        var innerExceptionMessage = NestedMessage(exception.InnerException);

        return $"{exception.Message}; {innerExceptionMessage}";
    }
}
