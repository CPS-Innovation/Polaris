using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Common.Handlers
{
    public interface IExceptionHandler
    {
        HttpResponseMessage HandleException(Exception exception, Guid correlationId, string source, ILogger logger);

        HttpResponseMessage HandleException(Exception exception, Guid correlationId, string source, ILogger logger, object obj);

        ObjectResult HandleExceptionNew(Exception exception, Guid correlationId, string source, ILogger logger);
    }
}
