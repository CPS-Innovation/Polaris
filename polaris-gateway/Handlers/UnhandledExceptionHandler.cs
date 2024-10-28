using System.Net;
using Common.Exceptions;
using Common.Logging;
using Ddei.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolarisGateway.Exceptions;

namespace PolarisGateway.Handlers;

public class UnhandledExceptionHandler : IUnhandledExceptionHandler
{
    public HttpResponseMessage HandleUnhandledException(
        ILogger logger,
        string logName,
        Guid correlationId,
        Exception ex,
        string additionalMessage = "")
    {
        var actionResponse = HandleUnhandledExceptionActionResult(logger, logName, correlationId, ex, additionalMessage);

        return new HttpResponseMessage()
        {
            StatusCode = (HttpStatusCode)actionResponse.StatusCode
        };
    }

    public StatusCodeResult HandleUnhandledExceptionActionResult(
        ILogger logger,
        string logName,
        Guid correlationId,
        Exception ex,
        string additionalMessage = "")
    {
        logger.LogMethodError(correlationId, logName, additionalMessage, ex);

        var statusCode = ex switch
        {
            DdeiClientException e => e.StatusCode switch
            {
                HttpStatusCode.Unauthorized => StatusCodes.Status401Unauthorized,
                HttpStatusCode.Forbidden => StatusCodes.Status403Forbidden,
                HttpStatusCode.Gone => StatusCodes.Status410Gone,
                HttpStatusCode.UnavailableForLegalReasons => StatusCodes.Status451UnavailableForLegalReasons,
                _ => StatusCodes.Status500InternalServerError
            },
            ArgumentNullException or BadRequestException _ => StatusCodes.Status400BadRequest,
            CpsAuthenticationException _ => StatusCodes.Status407ProxyAuthenticationRequired,
            _ => StatusCodes.Status500InternalServerError,
        };

        return new StatusCodeResult(statusCode);
    }
}