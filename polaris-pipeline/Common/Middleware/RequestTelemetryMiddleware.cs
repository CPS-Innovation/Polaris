using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Common.Extensions;
using Common.Telemetry;

namespace Common.Middleware;

public class RequestTelemetryMiddleware(Microsoft.ApplicationInsights.TelemetryClient telemetryClient) : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var requestTelemetry = new RequestTelemetry();
        requestTelemetry.Start();
        var requestData = await context.GetHttpRequestDataAsync();

        if (requestData is null)
        {
            await next(context);
            return;
        }

        var correlationId = Guid.NewGuid();
        try
        {
            correlationId = requestData.EstablishCorrelation();
        }
        catch
        {
        }

        if (context.BindingContext.BindingData.TryGetValue("documentId", out var documentId))
        {
            requestTelemetry.Properties[TelemetryConstants.DocumentIdCustomDimensionName] = documentId.ToString();
        }

        if (context.BindingContext.BindingData.TryGetValue("versionId", out var versionId))
        {
            requestTelemetry.Properties[TelemetryConstants.DocumentVersionIdCustomDimensionName] = versionId.ToString();
        }

        requestTelemetry.Properties[TelemetryConstants.CorrelationIdCustomDimensionName] = correlationId.ToString();

        await next(context);

        requestTelemetry.Context.Cloud.RoleName = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
        requestTelemetry.Context.Operation.Name = context.FunctionDefinition.Name;
        requestTelemetry.Name = context.FunctionDefinition.Name;
#pragma warning disable CS0618 // Type or member is obsolete
        requestTelemetry.HttpMethod = requestData.Method;
#pragma warning restore CS0618 // Type or member is obsolete
        requestTelemetry.ResponseCode = context.GetHttpResponseData()?.StatusCode.ToString() ?? string.Empty;
        requestTelemetry.Success = true;
        requestTelemetry.Url = requestData.Url;
        requestTelemetry.Stop();

        telemetryClient.TrackRequest(requestTelemetry);

        context.GetHttpResponseData()?.Headers.Add("Correlation-Id", correlationId.ToString());
    }
}
