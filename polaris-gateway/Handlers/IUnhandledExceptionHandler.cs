using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PolarisGateway.Handlers;

public interface IUnhandledExceptionHandler
{
    public HttpResponseMessage HandleUnhandledException(
        ILogger logger,
        string logName,
        Guid correlationId,
        Exception ex,
        string additionalMessage = "");

    public StatusCodeResult HandleUnhandledExceptionActionResult(
        ILogger logger,
        string logName,
        Guid correlationId,
        Exception ex,
        string additionalMessage = "");
}