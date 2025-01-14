using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
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
            TrackEventInternal(baseTelemetryEvent, isFailure: false);
        }

        public void TrackEventFailure(BaseTelemetryEvent baseTelemetryEvent)
        {
            TrackEventInternal(baseTelemetryEvent, isFailure: true);
        }

        private void TrackEventInternal(BaseTelemetryEvent baseTelemetryEvent, bool isFailure)
        {
            if (baseTelemetryEvent == null)
            {
                // As this is telemetry just silently fail
                // todo: a better/more assertive approach
                return;
            }


            var (properties, metrics) = baseTelemetryEvent.ToTelemetryEventProps();

            // filter metrics for only entries where we have a value
            var nonNullMetrics = metrics
                .Where(kvp => kvp.Value.HasValue)
                .ToDictionary(kvp => kvp.Key, kvp => (double)kvp.Value);

            properties.Add(telemetryVersion, Version);
            if (isFailure)
            {
                properties.Add("isFailure", "true");
            }

            var eventTelemetry = new EventTelemetry(PrepareEventName(baseTelemetryEvent.EventName));

            if (properties != null && properties.Count > 0)
            {
                CopyDictionary(PrepareKeyNames(properties), eventTelemetry.Properties);
            }

            if (nonNullMetrics != null && nonNullMetrics.Count > 0)
            {
                CopyDictionary(PrepareKeyNames(nonNullMetrics), eventTelemetry.Metrics);
            }

            eventTelemetry.Context.Operation.Name = baseTelemetryEvent.OperationName;
            eventTelemetry.Context.Cloud.RoleName = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");

            _telemetryClient.TrackEvent(eventTelemetry);
        }

        private static string PrepareEventName(string source) =>
            source.EndsWith("Event") ? source.Remove(source.LastIndexOf("Event")) : source;

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
            var propertyName = name
                // If the fields being captured are private and follow  _foo convention
                // then we need to remove the leading underscore
                .Replace("_", string.Empty);

            // If the fields being captured are public and follow Foo convention
            // then we need to lowercase the first character

            // Later note: going to lower case first char was not a good idea. In Log Analytics
            //  the convention seems to be Title case, and so in our Log Analytics functions (views)
            //  we are always converting back to title case.  A bit late now to change this.
            return ToLowerFirstChar(propertyName);
        }

        private static string ToLowerFirstChar(string input) =>
            string.IsNullOrEmpty(input) ? input : char.ToLower(input[0]) + input[1..];

        private static void CopyDictionary<TValue>(IDictionary<string, TValue> source, IDictionary<string, TValue> target)
        {
            foreach (var item in source)
            {
                if (!string.IsNullOrEmpty(item.Key) && !target.ContainsKey(item.Key))
                {
                    target[item.Key] = item.Value;
                }
            }
        }
    }
}
