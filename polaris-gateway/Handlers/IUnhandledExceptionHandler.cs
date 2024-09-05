using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PolarisGateway.Handlers;

public interface IUnhandledExceptionHandler
{
    public IActionResult HandleUnhandledException(
        ILogger logger,
        string logName,
        Guid correlationId,
        Exception ex,
        string additionalMessage = "");
}