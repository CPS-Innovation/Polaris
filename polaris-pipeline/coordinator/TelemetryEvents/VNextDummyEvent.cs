using System;
using System.Collections.Generic;
using Common.Telemetry;

namespace text_extractor.coordinator
{
    public class VNextDummyEvent : BaseTelemetryEvent
    {
        public Guid CorrelationId { get; set; }

        // todo this does not need to be nullable after first release to live
        public Guid? SubCorrelationId { get; set; }

        public string EventType { get; set; }

        public VNextDummyEvent(Guid correlationId, Guid? subCorrelationId, string eventType)
        {
            CorrelationId = correlationId;
            SubCorrelationId = subCorrelationId;
            EventType = eventType;
        }

        public override (IDictionary<string, string>, IDictionary<string, double?>) ToTelemetryEventProps()
        {
            return (
                new Dictionary<string, string>
                {
                    { nameof(CorrelationId), CorrelationId.ToString() },
                    { nameof(SubCorrelationId), SubCorrelationId.ToString() },
                    { nameof(EventType), EventType.ToString() },
                },
                new Dictionary<string, double?> { }
            );
        }
    }
}