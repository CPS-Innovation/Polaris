
using System;

namespace Common.Telemetry
{
    public interface ITelemetryClient
    {
        void TrackTrace(string message);

        void TrackException(Exception ex);

        void TrackEvent(BaseTelemetryEvent baseTelemetryEvent);

        void TrackEventFailure(BaseTelemetryEvent baseTelemetryEvent);
    }
}
