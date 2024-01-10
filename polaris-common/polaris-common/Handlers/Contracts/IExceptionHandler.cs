using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace polaris_common.Handlers.Contracts
{
    public interface IExceptionHandler
    {
        ObjectResult HandleException(Exception exception, Guid correlationId, string source, ILogger logger);
    }
}
