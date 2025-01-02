using Common.Extensions;
using Common.Middleware;
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
    .ConfigureFunctionsWebApplication(o =>
    {
        o.UseMiddleware<ExceptionHandlingMiddleware>();
        o.UseMiddleware<RequestValidationMiddleware>();
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
        services.AddApplicationInsightsTelemetryWorkerService();

        services.AddApplicationInsightsTelemetryWorkerService(new ApplicationInsightsServiceOptions
        {
            EnableAdaptiveSampling = false,
        });

        services.ConfigureFunctionsApplicationInsights();
        services.ConfigureLoggerFilterOptions();
    })
    .Build();

host.Run();

namespace ApplicationStartup
{
    public partial class Program;
}