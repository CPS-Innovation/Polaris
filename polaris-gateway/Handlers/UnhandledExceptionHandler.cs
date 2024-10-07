using System.Net;
using Common.Exceptions;
using Common.Extensions;
using Common.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PolarisGateway.Exceptions;

namespace PolarisGateway.Handlers;

public class UnhandledExceptionHandler : IUnhandledExceptionHandler
{
    public IActionResult HandleUnhandledException(
        ILogger logger,
        string logName,
        Guid correlationId,
        Exception ex,
        string additionalMessage = "")
    {
        logger.LogMethodError(correlationId, logName, additionalMessage, ex);

        var statusCode = ex switch
        {
            ArgumentNullException or BadRequestException _ => HttpStatusCode.BadRequest,
            CpsAuthenticationException _ => HttpStatusCode.ProxyAuthenticationRequired,
            _ => HttpStatusCode.InternalServerError,
        };
        
        return new ObjectResult(ex.ToStringFullResponse())
        {
            StatusCode = (int)statusCode
        };
    }
}