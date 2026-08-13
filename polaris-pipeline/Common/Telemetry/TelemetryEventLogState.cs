using System;

namespace Common.Telemetry
{
    /// <summary>
    /// Wraps a <see cref="BaseTelemetryEvent"/> so it can be passed through the <see cref="Microsoft.Extensions.Logging.ILogger"/>
    /// pipeline as structured state, whilst still allowing <see cref="TelemetryEventLoggerProvider"/> to reconstitute
    /// the equivalent Application Insights customEvent that was previously produced by ITelemetryClient.TrackEvent/TrackEventFailure.
    /// </summary>
    public sealed class TelemetryEventLogState
    {
        public TelemetryEventLogState(BaseTelemetryEvent telemetryEvent, bool isFailure)
        {
            TelemetryEvent = telemetryEvent ?? throw new ArgumentNullException(nameof(telemetryEvent));
            IsFailure = isFailure;
        }

        public BaseTelemetryEvent TelemetryEvent { get; }

        public bool IsFailure { get; }

        public override string ToString() =>
            $"TelemetryEvent: {TelemetryEvent.EventName}{(IsFailure ? " (failure)" : string.Empty)}";
    }
}
