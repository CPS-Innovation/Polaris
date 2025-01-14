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
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Common.Telemetry;

namespace Common.Middleware;

public class ExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly Microsoft.ApplicationInsights.TelemetryClient _telemetryClient;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, Microsoft.ApplicationInsights.TelemetryClient telemetryClient)
    {
        _logger = logger;
        _telemetryClient = telemetryClient;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var requestTelemetry = new RequestTelemetry();
        requestTelemetry.Start();

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

                _logger.LogMethodError(correlationId, httpRequestData.Url.ToString(), message, exception);
                requestTelemetry.Properties[TelemetryConstants.CorrelationIdCustomDimensionName] = correlationId.ToString();

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

                requestTelemetry.Context.Cloud.RoleName = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
                requestTelemetry.Context.Operation.Name = context.FunctionDefinition.Name;
                requestTelemetry.Name = context.FunctionDefinition.Name;
                requestTelemetry.HttpMethod = httpRequestData.Method;
                requestTelemetry.ResponseCode = ((int)statusCode).ToString();
                requestTelemetry.Success = false;
                requestTelemetry.Url = httpRequestData.Url;
                requestTelemetry.Properties[TelemetryConstants.ErrorMessageCustomDimensionName] = exception.ToStringFullResponse();
                requestTelemetry.Stop();

                _telemetryClient.TrackRequest(requestTelemetry);
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