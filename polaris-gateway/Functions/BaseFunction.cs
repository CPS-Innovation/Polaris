using Common.Telemetry;
using Microsoft.AspNetCore.Mvc;

namespace PolarisGateway.Functions;

public abstract class BaseFunction(ITelemetryClient telemetryClient)
{
    protected IActionResult SendTelemetryAndReturn(BaseTelemetryEvent telemetryEvent, IActionResult result)
    {
        telemetryClient.TrackEvent(telemetryEvent);
        return result;
    }

    protected IActionResult SendTelemetryAndReturnBadRequest(BaseTelemetryEvent telemetryEvent)
    {
        telemetryClient.TrackEvent(telemetryEvent);
        return new BadRequestResult();
    }
}