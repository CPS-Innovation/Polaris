
namespace Common.Telemetry.Contracts
{
    public interface ITelemetryClient
    {
        void TrackEvent(BaseTelemetryEvent baseTelemetryEvent);

        void TrackEventFailure(BaseTelemetryEvent baseTelemetryEvent);
    }
}
