using System.Linq;
using Common.Middleware;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
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
        services.ConfigureFunctionsApplicationInsights();
        services.Configure<LoggerFilterOptions>(options =>
        {
            // See: https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=windows#managing-log-levels
            // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs. Application Insights requires an explicit override.
            // Log levels can also be configured using appsettings.json. For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
            var toRemove = options.Rules
              .FirstOrDefault(rule => string.Equals(rule.ProviderName, typeof(ApplicationInsightsLoggerProvider).FullName));

            if (toRemove is not null)
            {
                options.Rules.Remove(toRemove);
            }
        });
    })
    .Build();

host.Run();

namespace ApplicationStartup
{
    public partial class Program;
}