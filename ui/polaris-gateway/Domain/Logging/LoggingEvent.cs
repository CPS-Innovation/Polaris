using Ardalis.SmartEnum;

namespace RumpoleGateway.Domain.Logging;

public sealed class LoggingEvent : SmartEnum<LoggingEvent>
{
    public static readonly LoggingEvent MethodEntry = new ("Method entry", 1001);
    public static readonly LoggingEvent MethodExit = new ("Method exit", 1002);
    public static readonly LoggingEvent MethodFlowHighlight = new ("Method Flow Highlight", 1003);
    public static readonly LoggingEvent ProcessingFailed = new ("Processing Failed", 1004);

    private LoggingEvent(string name, int value) : base(name, value)
    {
    }
}