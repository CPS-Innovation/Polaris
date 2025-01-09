using Common.Extensions;
using Common.Telemetry;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using System;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using System.Text.RegularExpressions;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights;

namespace coordinator.Middleware;

public sealed partial class RequestValidationMiddleware(Microsoft.ApplicationInsights.TelemetryClient telemetryClient) : IFunctionsWorkerMiddleware
{
    private const int MockUserUserId = int.MinValue;

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var requestTelemetry = new RequestTelemetry();
        requestTelemetry.Start();
        var requestData = await context.GetHttpRequestDataAsync();
        var correlationId = Guid.NewGuid();

        if (requestData is null)
        {
            await next(context);
            return;
        }

        try
        {
            correlationId = requestData.EstablishCorrelation();
        }
        catch
        {
            if (!((requestData.Url.LocalPath.Equals($"/api/{RestApi.Case}", StringComparison.InvariantCultureIgnoreCase) && requestData.Method.Equals("delete", StringComparison.InvariantCultureIgnoreCase)) ||
                requestData.Url.LocalPath.Equals($"/api/{RestApi.Status}", StringComparison.InvariantCultureIgnoreCase)))
            {
                throw;
            }
        }

        var refreshCaseUrlRegex = RefreshCaseUrlRegex();
        var redactDocumentUrlRegex = RedactDocumentUrlRegex();
        var modifyDocumentUrlRegex = ModifyDocumentUrlRegex();

        if ((refreshCaseUrlRegex.IsMatch(requestData.Url.LocalPath) && requestData.Method.Equals("post", StringComparison.InvariantCultureIgnoreCase)) ||
            redactDocumentUrlRegex.IsMatch(requestData.Url.LocalPath) ||
            modifyDocumentUrlRegex.IsMatch(requestData.Url.LocalPath))
        {
            var cmsAuthValues = requestData.EstablishCmsAuthValuesFromHeaders();
            var cmsUserId = cmsAuthValues.ExtractCmsUserId();
            var isMockUser = cmsUserId == MockUserUserId;

            requestTelemetry.Properties[TelemetryConstants.CmsUserIdCustomDimensionName] = cmsUserId.ToString();

            if (isMockUser)
            {
                requestTelemetry.Properties[TelemetryConstants.IsMockUser] = true.ToString();
            }
        }

        await next(context);

        requestTelemetry.Context.Operation.Name = context.FunctionDefinition.Name;
        requestTelemetry.Properties[TelemetryConstants.CorrelationIdCustomDimensionName] = correlationId.ToString();
        requestTelemetry.Name = context.FunctionDefinition.Name;
        requestTelemetry.HttpMethod = requestData.Method;
        requestTelemetry.ResponseCode = context.GetHttpResponseData()?.StatusCode.ToString() ?? string.Empty;
        requestTelemetry.Success = true;
        requestTelemetry.Url = requestData.Url;
        requestTelemetry.Stop();

        telemetryClient.TrackRequest(requestTelemetry);

        context.GetHttpResponseData()?.Headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());
    }

    [GeneratedRegex("/api/urns/([a-zA-Z\\d]+)/cases/\\d+", RegexOptions.IgnoreCase)]
    private static partial Regex RefreshCaseUrlRegex();

    [GeneratedRegex("/api/urns/([a-zA-Z\\d]+)/cases/\\d+/documents/[^/]+/modify", RegexOptions.IgnoreCase)]
    private static partial Regex ModifyDocumentUrlRegex();

    [GeneratedRegex("/api/urns/([a-zA-Z\\d]+)/cases/\\d+/documents/[^/]+/redact", RegexOptions.IgnoreCase)]
    private static partial Regex RedactDocumentUrlRegex();
}