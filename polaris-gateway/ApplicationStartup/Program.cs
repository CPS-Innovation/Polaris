using Common.Extensions;
using Common.Middleware;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using PolarisGateway.ApplicationStartup;
using PolarisGateway.Middleware;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(options =>
    {
        options.UseMiddleware<ExceptionHandlingMiddleware>();
        options.UseMiddleware<RequestValidationMiddleware>();
    })
    .ConfigureOpenApi()
    .ConfigureLogging(options => options.AddApplicationInsights())
    .ConfigureAppConfiguration(builder => builder.AddConfigurationSettings())
    .ConfigureServices((services) =>
    {
#if DEBUG
        // https://stackoverflow.com/questions/54435551/invalidoperationexception-idx20803-unable-to-obtain-configuration-from-pii
        IdentityModelEventSource.ShowPII = true;
        IdentityModelEventSource.LogCompleteSecurityArtifact = true;
#endif
        services.ConfigureServices();
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
    })
    .Build();

host.Run();

namespace ApplicationStartup
{
    public partial class Program;
}