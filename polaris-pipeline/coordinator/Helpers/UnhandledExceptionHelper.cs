using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Common.Logging;
using Common.Domain.Exceptions;
using Microsoft.AspNetCore.Http;

namespace coordinator.Helpers;

// todo: temporary code during refactor
public static class UnhandledExceptionHelper
{
    public static IActionResult HandleUnhandledException(ILogger logger, string logName, Guid correlationId, Exception ex)
    {
        logger.LogMethodError(correlationId, logName, ex.Message, ex);

        return ex switch
        {
            BadRequestException => new StatusCodeResult(StatusCodes.Status400BadRequest),
            _ => new StatusCodeResult(StatusCodes.Status500InternalServerError)
        };
    }
}