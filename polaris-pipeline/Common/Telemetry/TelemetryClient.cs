using System.Collections.Generic;
using Common.Telemetry.Contracts;
using AppInsights = Microsoft.ApplicationInsights;

namespace Common.Telemetry
{
    public class TelemetryClient : ITelemetryClient
    {
        public const string telemetryVersion = nameof(telemetryVersion);

        public const string Version = "0.1";

        protected readonly AppInsights.TelemetryClient _telemetryClient;

        public TelemetryClient(AppInsights.TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        public void TrackEvent(BaseTelemetryEvent baseTelemetryEvent)
        {
            var (properties, metrics) = baseTelemetryEvent.ToTelemetryEventProps();
            properties.Add(telemetryVersion, Version);

            _telemetryClient.TrackEvent(
                PrepareEventName(baseTelemetryEvent.EventName),
                PrepareKeyNames(properties),
                PrepareKeyNames(metrics)
            );
        }
        private static string PrepareEventName(string source)
        {
            if (!source.EndsWith("Event"))
                return source;

            return source.Remove(source.LastIndexOf("Event"));
        }

        private static IDictionary<string, T> PrepareKeyNames<T>(IDictionary<string, T> properties)
        {
            var cleanedProperties = new Dictionary<string, T>();

            foreach (var property in properties)
            {
                cleanedProperties.Add(CleanPropertyName(property.Key), property.Value);
            }

            return cleanedProperties;
        }

        private static string CleanPropertyName(string name)
        {
            return name.Replace("_", string.Empty);
        }
    }
}
