using Common.Exceptions;
using Common.Extensions;
using Common.Logging;
using Common.Telemetry;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Abstractions;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

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
        var operation = _telemetryClient.StartOperation<RequestTelemetry>(
            context.FunctionDefinition.Name);

        try
        {
            await next(context);

            // Success case
            operation.Telemetry.Success = true;
            operation.Telemetry.ResponseCode = "200";
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
                OcrDocumentNotFoundException _ => HttpStatusCode.NotFound,
                DocumentNotFoundException _ => HttpStatusCode.NotFound,
                HttpRequestException _ => HttpStatusCode.BadGateway,
                _ => HttpStatusCode.InternalServerError,
            };

            var httpRequestData = await context.GetHttpRequestDataAsync();

            if (httpRequestData != null)
            {
                var correlationId = Guid.NewGuid();
                try
                {
                    correlationId = httpRequestData.EstablishCorrelation();
                }
                catch { }

                _logger.LogMethodError(
                    correlationId,
                    httpRequestData.Url.ToString(),
                    string.Empty,
                    exception);

                // Build response
                var newHttpResponse = httpRequestData.CreateResponse(statusCode);

                await newHttpResponse.WriteAsJsonAsync(new
                {
                    ErrorMessage = exception.ToStringFullResponse(),
                    CorrelationId = correlationId
                });

                var invocationResult = context.GetInvocationResult();

                var httpOutputBinding = GetHttpOutputBindingFromMultipleOutputBinding(context);
                if (httpOutputBinding is not null)
                {
                    httpOutputBinding.Value = newHttpResponse;
                }
                else
                {
                    invocationResult.Value = newHttpResponse;
                }

                // ✅ Enrich telemetry (NO manual RequestTelemetry object)
                operation.Telemetry.Success = false;
                operation.Telemetry.ResponseCode = ((int)statusCode).ToString();
                operation.Telemetry.Url = httpRequestData.Url;

                // Recommended naming convention
                operation.Telemetry.Name = $"{httpRequestData.Method} {context.FunctionDefinition.Name}";

                // Custom properties
                operation.Telemetry.Properties[TelemetryConstants.CorrelationIdCustomDimensionName] = correlationId.ToString();
                operation.Telemetry.Properties[TelemetryConstants.ErrorMessageCustomDimensionName] = exception.ToStringFullResponse();
                operation.Telemetry.Properties["HttpMethod"] = httpRequestData.Method;
            }

            throw; // preserve original behavior
        }
        finally
        {
            _telemetryClient.StopOperation(operation);
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