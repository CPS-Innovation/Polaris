using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Common.Logging;
using Common.Exceptions;
using Microsoft.AspNetCore.Http;
using DdeiClient.Exceptions;

namespace coordinator.Helpers;

// todo: temporary code during refactor
public static class UnhandledExceptionHelper
{
    public static IActionResult HandleUnhandledException(ILogger logger, string logName, Guid correlationId, Exception ex)
    {
        logger.LogMethodError(correlationId, logName, ex.Message, ex);

        return ex switch
        {
            // For the time being if we get 401 from DDEI, we transmit this to the client,
            //  otherwise this is an unexpected error and we return 500
            DdeiClientException e => new StatusCodeResult(e.StatusCode == System.Net.HttpStatusCode.Unauthorized
                                        ? StatusCodes.Status401Unauthorized
                                        : StatusCodes.Status500InternalServerError),
            BadRequestException => new StatusCodeResult(StatusCodes.Status400BadRequest),
            _ => new StatusCodeResult(StatusCodes.Status500InternalServerError)
        };
    }
}