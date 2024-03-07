using System;

namespace Common.Telemetry
{
    public class CriticalTelemetryException : Exception
    {
        public CriticalTelemetryException(string message, Exception exception = null)
        : base(message, exception)
        {
        }
    }
}