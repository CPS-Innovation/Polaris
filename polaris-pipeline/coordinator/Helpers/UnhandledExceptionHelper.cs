using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Common.Logging;
using Common.Exceptions;
using Microsoft.AspNetCore.Http;
using DdeiClient.Exceptions;

namespace coordinator.Helpers
{
    // todo: temporary code during refactor
    public static class UnhandledExceptionHelper
    {
        public static IActionResult HandleUnhandledException(ILogger logger, string logName, Guid correlationId, Exception ex)
        {
            logger.LogMethodError(correlationId, logName, ex.Message, ex);

            return ex switch
            {
                DdeiClientException e => new StatusCodeResult(e.StatusCode switch
                {
                    System.Net.HttpStatusCode.Unauthorized => StatusCodes.Status401Unauthorized,
                    System.Net.HttpStatusCode.Forbidden => StatusCodes.Status403Forbidden,
                    System.Net.HttpStatusCode.Gone => StatusCodes.Status410Gone,
                    System.Net.HttpStatusCode.UnavailableForLegalReasons => StatusCodes.Status451UnavailableForLegalReasons,
                    _ => StatusCodes.Status500InternalServerError
                }),
                BadRequestException => new StatusCodeResult(StatusCodes.Status400BadRequest),
                _ => new StatusCodeResult(StatusCodes.Status500InternalServerError)
            };
        }
    }
}
