using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Common.Telemetry
{
    public abstract class BaseTelemetryEvent
    {
        protected const string durationSeconds = nameof(durationSeconds);

        public string EventName
        {
            get
            {
                return GetType().Name;
            }
        }
        abstract public (IDictionary<string, string>, IDictionary<string, double?>) ToTelemetryEventProps();

        public static double? GetDurationSeconds(DateTime startTime, DateTime endTime)
        {
            if (startTime == default || endTime == default)
                return null;

            return (double)(endTime - startTime).TotalSeconds;
        }

        public static string EnsureNumericId(string documentId)
        {
            return Regex.Match(
                // let's cope with nulls, this is logging logic, not business
                documentId ?? string.Empty,
                @"\d+",
                RegexOptions.None,
                // avoid DOS attacks, keep code scanning happy
                TimeSpan.FromMilliseconds(100)
            ).Value;
        }
    }
}
