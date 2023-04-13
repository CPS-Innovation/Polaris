using System;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Common.Handlers.Contracts
{
    public interface IExceptionHandler
    {
        HttpResponseMessage HandleException(Exception exception, Guid correlationId, string source, ILogger logger);
    }
}
