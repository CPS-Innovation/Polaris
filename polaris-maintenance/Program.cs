using Common.Extensions;
using Common.Middleware;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(options =>
    {
        options.UseMiddleware<ExceptionHandlingMiddleware>();
        options.UseMiddleware<RequestTelemetryMiddleware>();
    })
    .ConfigureHostConfiguration(builder => builder.AddConfigurationSettings())
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService(new ApplicationInsightsServiceOptions
        {
            EnableAdaptiveSampling = false,
        });
        services.ConfigureFunctionsApplicationInsights();
        // Commented out to match WMA instance of DDEI configuration and see if it improves log visibility
        /* services.Configure<TelemetryConfiguration>(telemetryConfiguration =>
        {
            telemetryConfiguration.DefaultTelemetrySink.TelemetryProcessorChainBuilder
                .UseAdaptiveSampling(maxTelemetryItemsPerSecond: 20, excludedTypes: "Request;Exception;Event;Trace");
            telemetryConfiguration.DisableTelemetry = false;
        }); */
        services.ConfigureLoggerFilterOptions();
        services.AddSingleton<Microsoft.ApplicationInsights.TelemetryClient, Microsoft.ApplicationInsights.TelemetryClient>();
    })
    .Build();

host.Run();
