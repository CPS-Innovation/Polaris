using Common.Exceptions;
using Common.Extensions;
using Common.Logging;
using Common.Telemetry;
using Cps.MasterDataService.Infrastructure.ApiClient;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
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
                BadRequestException or ArgumentNullException _ => HttpStatusCode.BadRequest,
                DdeiClientException e => e.StatusCode,
                CpsAuthenticationException _ => HttpStatusCode.ProxyAuthenticationRequired,
                CmsAuthValuesMissingException _ => HttpStatusCode.Unauthorized,
                OcrDocumentNotFoundException or DocumentNotFoundException _ => HttpStatusCode.NotFound,
                ApiException e => (HttpStatusCode)e.StatusCode,
                HttpRequestException e => (HttpStatusCode)e.StatusCode, 
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

                // Add ClientName to telemetry if exists in header
                if (httpRequestData.Headers.TryGetValues("ClientName", out var clientNames))
                {
                    requestTelemetry.Properties[TelemetryConstants.ClientNameCustomDimensionName] = clientNames.FirstOrDefault(string.Empty);
                }

                var newHttpResponse = httpRequestData.CreateResponse(statusCode);

                var errorMessage = ExtractErrorMessage(exception);

                await newHttpResponse.WriteAsJsonAsync(new { Error = errorMessage, AdditionalDetails = exception.ToStringFullResponse(), CorrelationId = correlationId });

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
#pragma warning disable CS0618 // Type or member is obsolete
                requestTelemetry.HttpMethod = httpRequestData.Method;
#pragma warning restore CS0618 // Type or member is obsolete
                requestTelemetry.ResponseCode = ((int)statusCode).ToString();
                requestTelemetry.Success = false;
                requestTelemetry.Url = httpRequestData.Url;
                requestTelemetry.Properties[TelemetryConstants.ErrorMessageCustomDimensionName] = exception.ToStringFullResponse();
                requestTelemetry.Stop();

                _telemetryClient.TrackRequest(requestTelemetry);
            }
        }
    }

    private static string ExtractErrorMessage(Exception exception)
    {
        var fullExceptionText = exception.InnerException?.Message ?? exception.Message;

        if (string.IsNullOrWhiteSpace(fullExceptionText))
        {
            return string.Empty;
        }

        const string startToken = "Exception Message:";
        const string endToken = "\",\"traceId\"";

        var startIndex = fullExceptionText.IndexOf(
            startToken,
            StringComparison.OrdinalIgnoreCase);

        if (startIndex < 0)
        {
            return fullExceptionText;
        }

        startIndex += startToken.Length;

        if (startIndex >= fullExceptionText.Length)
        {
            return string.Empty;
        }

        var endIndex = fullExceptionText.IndexOf(
            endToken,
            startIndex,
            StringComparison.OrdinalIgnoreCase);

        if (endIndex < 0 || endIndex <= startIndex)
        {
            return fullExceptionText[startIndex..].Trim();
        }

        return fullExceptionText[startIndex..endIndex].Trim();
    }

    private static OutputBindingData<HttpResponseData> GetHttpOutputBindingFromMultipleOutputBinding(FunctionContext context)
    {
        // The output binding entry name will be "$return" only when the function return type is HttpResponseData
        var httpOutputBinding = context.GetOutputBindings<HttpResponseData>()
            .FirstOrDefault(b => b.BindingType == "http" && b.Name != "$return");

        return httpOutputBinding;
    }
}