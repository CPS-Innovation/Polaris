// <copyright file="TelemetryEventLoggerProvider.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using AppInsights = Microsoft.ApplicationInsights;

namespace Common.Telemetry
{
    /// <summary>
    /// An <see cref="ILoggerProvider"/> that inspects log entries raised via <see cref="TelemetryEventLoggerExtensions"/>
    /// and converts them into Application Insights customEvents, preserving the shape previously produced by
    /// Common.Telemetry.TelemetryClient
    /// </summary>
    public sealed class TelemetryEventLoggerProvider : ILoggerProvider
    {
        public const int TelemetryEventId = 90210;

        public const string telemetryVersion = nameof(telemetryVersion);

        public const string Version = "0.1";

        private readonly AppInsights.TelemetryClient _telemetryClient;

        public TelemetryEventLoggerProvider(AppInsights.TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        public ILogger CreateLogger(string categoryName) => new TelemetryEventLogger(_telemetryClient);

        public void Dispose()
        {
        }

        private sealed class TelemetryEventLogger : ILogger
        {
            private readonly AppInsights.TelemetryClient _telemetryClient;

            public TelemetryEventLogger(AppInsights.TelemetryClient telemetryClient)
            {
                _telemetryClient = telemetryClient;
            }

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (state is not TelemetryEventLogState telemetryEventLogState)
                {
                    return;
                }

                TrackEventInternal(telemetryEventLogState.TelemetryEvent, telemetryEventLogState.IsFailure);
            }

            private void TrackEventInternal(BaseTelemetryEvent baseTelemetryEvent, bool isFailure)
            {
                if (baseTelemetryEvent == null)
                {
                    return;
                }

                var (properties, metrics) = baseTelemetryEvent.ToTelemetryEventProps();

                // filter metrics for only entries where we have a value
                var nonNullMetrics = metrics
                    .Where(kvp => kvp.Value.HasValue)
                    .ToDictionary(kvp => kvp.Key, kvp => (double)kvp.Value);

                properties.Add(TelemetryEventLoggerProvider.telemetryVersion, TelemetryEventLoggerProvider.Version);
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
                    .Replace("_", string.Empty);

                // If the fields being captured are public
                // then we need to lowercase the first character
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

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();

                public void Dispose()
                {
                }
            }
        }
    }
}
