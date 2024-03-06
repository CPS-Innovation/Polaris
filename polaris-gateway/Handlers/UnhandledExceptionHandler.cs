using System.Net;
using Common.Domain.Exceptions;
using Common.Exceptions;
using Common.Logging;
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
        logger.LogMethodError(correlationId, logName, additionalMessage, ex);

        // todo: PipelineClient exceptions
        var statusCode = ex switch
        {
            ArgumentNullException or BadRequestException _ => HttpStatusCode.BadRequest,
            CpsAuthenticationException _ => HttpStatusCode.Unauthorized,
            // todo: as refactor goes on we will lose reference to DdeiClientException
            // must be 403, client will react to a 403 to trigger reauthentication
            DdeiClientException or CmsAuthenticationException _ => HttpStatusCode.Forbidden,
            _ => HttpStatusCode.InternalServerError,
        };

        return new HttpResponseMessage() { StatusCode = statusCode };
    }
}