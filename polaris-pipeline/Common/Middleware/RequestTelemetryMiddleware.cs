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
        var requestData = await context.GetHttpRequestDataAsync();

        // If not an HTTP trigger, just continue
        if (requestData is null)
        {
            await next(context);
            return;
        }

        var operation = telemetryClient.StartOperation<RequestTelemetry>(
            context.FunctionDefinition.Name);
        var requestTelemetry = operation.Telemetry;
        var correlationId = Guid.NewGuid();

        correlationId = requestData.EstablishCorrelation();


        try
        {
            // Add correlation early
            operation.Telemetry.Properties[TelemetryConstants.CorrelationIdCustomDimensionName] =
                correlationId.ToString();

            // Add binding data if present
            if (context.BindingContext.BindingData.TryGetValue("documentId", out var documentId))
            {
                operation.Telemetry.Properties[TelemetryConstants.DocumentIdCustomDimensionName] =
                    documentId?.ToString();
            }

            if (context.BindingContext.BindingData.TryGetValue("versionId", out var versionId))
            {
                operation.Telemetry.Properties[TelemetryConstants.DocumentVersionIdCustomDimensionName] =
                    versionId?.ToString();
            }

            await next(context);

            var response = context.GetHttpResponseData();

            // ✅ Populate telemetry AFTER execution
            operation.Telemetry.Success = true;
            operation.Telemetry.ResponseCode = response?.StatusCode.ToString() ?? "200";
            operation.Telemetry.Url = requestData.Url;

            // Recommended naming
            operation.Telemetry.Name =
                $"{requestData.Method} {context.FunctionDefinition.Name}";
        }
        catch
        {
            operation.Telemetry.Success = false;
            operation.Telemetry.ResponseCode = "500";
            throw;
        }
        finally
        {
            telemetryClient.StopOperation(operation);

            // Add correlation header to response
            context.GetHttpResponseData()?.Headers.Add(
                "Correlation-Id",
                correlationId.ToString());
        }
    }
}
