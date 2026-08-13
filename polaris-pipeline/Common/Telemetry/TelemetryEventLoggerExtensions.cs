using Microsoft.Extensions.Logging;

namespace Common.Telemetry
{
    /// <summary>
    /// Extension methods that let callers use ILogger to raise business event telemetry, replacing the previous
    /// ITelemetryClient.TrackEvent/TrackEventFailure API. The events raised through these methods are picked up by
    /// <see cref="TelemetryEventLoggerProvider"/> and converted into Application Insights customEvents, preserving
    /// the shape (name, properties, metrics, telemetryVersion, isFailure flag) used for business reporting.
    /// </summary>
    public static class TelemetryEventLoggerExtensions
    {
        public static readonly EventId TelemetryEventId = new(TelemetryEventLoggerProvider.TelemetryEventId, nameof(TrackEvent));

        public static void TrackEvent(this ILogger logger, BaseTelemetryEvent telemetryEvent)
        {
            if (telemetryEvent is null)
            {
                // As this is telemetry just silently fail
                // todo: a better/more assertive approach
                return;
            }

            var state = new TelemetryEventLogState(telemetryEvent, isFailure: false);
            logger.Log(LogLevel.Information, TelemetryEventId, state, null, (s, _) => s.ToString());
        }

        public static void TrackEventFailure(this ILogger logger, BaseTelemetryEvent telemetryEvent)
        {
            if (telemetryEvent is null)
            {
                // As this is telemetry just silently fail
                // todo: a better/more assertive approach
                return;
            }

            var state = new TelemetryEventLogState(telemetryEvent, isFailure: true);
            logger.Log(LogLevel.Information, TelemetryEventId, state, null, (s, _) => s.ToString());
        }
    }
}
