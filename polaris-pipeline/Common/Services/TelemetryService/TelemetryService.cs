
// using Microsoft.ApplicationInsights;
// using Microsoft.ApplicationInsights.Extensibility;

namespace Common.Services.TelemetryService;

public class TelemetryService
{
    // private readonly TelemetryClient _telemetryClient;

    // public TelemetryService(TelemetryConfiguration telemetryConfiguration)
    // {
    //     _telemetryClient = new TelemetryClient(telemetryConfiguration);
    // }

    public void TrackDocumentDocumentConversion()
    {

    }
}

/*
DateTime start = DateTime.UtcNow;
            // Write an event to the customEvents table.
            var evt = new EventTelemetry("Function called");
            evt.Name = "AppInsightsTest";
            evt.Properties["some_custom_property"] = "some value";
            evt.Properties["cms_case_id"] = payload.CmsCaseId.ToString();
            evt.Properties["cms_case_urn"] = payload.CmsCaseUrn;
            evt.Metrics["some_custom_metric"] = 42;
            _telemetryClient.TrackEvent(evt);

            _telemetryClient.GetMetric("some_independent_metric").TrackValue(43);


                // Log a custom dependency in the dependencies table.
                var dependency = new DependencyTelemetry
                {
                    Name = "GET api/planets/1/",
                    Target = "swapi.co",
                    Data = "https://swapi.co/api/planets/1/",
                    Timestamp = start,
                    Duration = DateTime.UtcNow - start,
                    Success = true
                };

                _telemetryClient.TrackDependency(dependency);
*/