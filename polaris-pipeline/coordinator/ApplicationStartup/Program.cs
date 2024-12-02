using System.Linq;
using System.Text.Json;
using Azure.Core.Serialization;
using coordinator.ApplicationStartup;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(o =>
    {
        o.Services.Configure<WorkerOptions>(workerOptions =>
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            };

            workerOptions.Serializer = new JsonObjectSerializer(options);
        });

        //o.UseMiddleware<ExceptionHandlingMiddleware>();
        //o.UseMiddleware<RequestValidationMiddleware>();
    })
    .ConfigureOpenApi()
    .ConfigureLogging(options => options.AddApplicationInsights())
    .ConfigureAppConfiguration(builder => builder.AddConfigurationSettings())
    .ConfigureServices((services) =>
    {
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

        services.AddMvc().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
    })
    .Build();

host.Run();

namespace ApplicationStartup
{
    public partial class Program;
}
