namespace polaris_common.Domain.Exceptions
{
    public class CriticalTelemetryException : Exception
    {
        public CriticalTelemetryException(string message, Exception exception = null)
        : base(message, exception)
        {
        }
    }
}