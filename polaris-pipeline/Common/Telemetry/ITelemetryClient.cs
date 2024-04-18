
namespace Common.Telemetry
{
    public interface ITelemetryClient
    {
        void TrackEvent(BaseTelemetryEvent baseTelemetryEvent);

        void TrackEventFailure(BaseTelemetryEvent baseTelemetryEvent);
    }
}
