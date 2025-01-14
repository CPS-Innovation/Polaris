using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Common.Extensions;
using Common.Telemetry;

namespace Common.Middleware;

public class RequestTelemetryMiddleware : IFunctionsWorkerMiddleware
{
    private readonly Microsoft.ApplicationInsights.TelemetryClient _telemetryClient;

    public RequestTelemetryMiddleware(Microsoft.ApplicationInsights.TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

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
        requestTelemetry.HttpMethod = requestData.Method;
        requestTelemetry.ResponseCode = context.GetHttpResponseData()?.StatusCode.ToString() ?? string.Empty;
        requestTelemetry.Success = true;
        requestTelemetry.Url = requestData.Url;
        requestTelemetry.Stop();

        _telemetryClient.TrackRequest(requestTelemetry);

        context.GetHttpResponseData()?.Headers.Add("Correlation-Id", correlationId.ToString());
    }
}
