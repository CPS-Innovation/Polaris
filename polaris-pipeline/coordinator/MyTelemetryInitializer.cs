using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace coordinator;
internal class MyTelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        //use telemetry is RequestTelemetry to make sure only add to request
        if (telemetry != null)
        {
            if (!telemetry.Context.GlobalProperties.ContainsKey("AppInsightsTest"))
            {
                telemetry.Context.GlobalProperties.Add("AppInsightsTest", "Test123");
            }

            if (telemetry is RequestTelemetry && !telemetry.Context.GlobalProperties.ContainsKey("AppInsightsTestUrl") && (telemetry as RequestTelemetry).Url != null)
            {
                telemetry.Context.GlobalProperties.Add("AppInsightsTestUrl", (telemetry as RequestTelemetry).Url.ToString());
            }
        }
    }
}