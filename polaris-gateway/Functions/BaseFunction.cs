using Common.Constants;
using Common.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PolarisGateway.Extensions;
using PolarisGateway.TelemetryEvents;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PolarisGateway.Functions;

public class BaseFunction(ITelemetryClient telemetryClient)
{
    protected async Task<IActionResult> SendTelemetryAndReturn(HttpResponseMessage result, BaseRequestEvent telemetryEvent)
    {
        telemetryClient.TrackEvent(telemetryEvent);
        return await result.ToActionResult();
    }

    protected static Guid EstablishCorrelation(HttpRequest req) =>
        req.Headers.TryGetValue(HttpHeaderKeys.CorrelationId, out var correlationId) &&
        Guid.TryParse(correlationId, out var parsedCorrelationId) ?
            parsedCorrelationId :
            Guid.NewGuid();

    protected static string EstablishCmsAuthValues(HttpRequest req)
    {
        if (req.Headers.TryGetValue(HttpHeaderKeys.CmsAuthValues, out var cmsAuthValuesFromHeaders) &&
            cmsAuthValuesFromHeaders.Any() &&
            !string.IsNullOrWhiteSpace(cmsAuthValuesFromHeaders.First()))
        {
            // traffic from production pipeline functions
            return cmsAuthValuesFromHeaders.First();
        }

        req.Cookies.TryGetValue(HttpHeaderKeys.CmsAuthValues, out var cmsAuthValues);
        if (cmsAuthValues != null)
        {
            return cmsAuthValues;
        }
        return null;
    }
}