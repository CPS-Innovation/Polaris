using Common.Exceptions;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using System;
using System.Linq;
using Common.Logging;
using Common.Extensions;

namespace Common.Middleware;

public class ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger) : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var statusCode = exception switch
            {
                BadRequestException _ => HttpStatusCode.BadRequest,
                DdeiClientException e => e.StatusCode,
                ArgumentNullException or BadRequestException _ => HttpStatusCode.BadRequest,
                CpsAuthenticationException _ => HttpStatusCode.ProxyAuthenticationRequired,
                CmsAuthValuesMissingException _ => HttpStatusCode.Unauthorized,
                _ => HttpStatusCode.InternalServerError,
            };

            var message = string.Empty;

            var httpRequestData = await context.GetHttpRequestDataAsync();

            if (httpRequestData != null)
            {
                var correlationId = Guid.NewGuid();
                try
                {
                    correlationId = httpRequestData.EstablishCorrelation();
                }
                catch
                {
                }

                logger.LogMethodError(correlationId, httpRequestData.Url.ToString(), message, exception);

                var newHttpResponse = httpRequestData.CreateResponse(statusCode);

                await newHttpResponse.WriteAsJsonAsync(new { ErrorMessage = exception.ToStringFullResponse(), CorrelationId = correlationId });

                var invocationResult = context.GetInvocationResult();

                var httpOutputBindingFromMultipleOutputBindings = GetHttpOutputBindingFromMultipleOutputBinding(context);
                if (httpOutputBindingFromMultipleOutputBindings is not null)
                {
                    httpOutputBindingFromMultipleOutputBindings.Value = newHttpResponse;
                }
                else
                {
                    invocationResult.Value = newHttpResponse;
                }
            }
        }
    }

    private static OutputBindingData<HttpResponseData> GetHttpOutputBindingFromMultipleOutputBinding(FunctionContext context)
    {
        // The output binding entry name will be "$return" only when the function return type is HttpResponseData
        var httpOutputBinding = context.GetOutputBindings<HttpResponseData>()
            .FirstOrDefault(b => b.BindingType == "http" && b.Name != "$return");

        return httpOutputBinding;
    }
}
