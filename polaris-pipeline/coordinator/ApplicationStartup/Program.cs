using System.Text.Json;
using Azure.Core.Serialization;
using Common.Extensions;
using Common.Middleware;
using coordinator.ApplicationStartup;
using coordinator.Middleware;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

        o.UseMiddleware<ExceptionHandlingMiddleware>();
        o.UseMiddleware<RequestValidationMiddleware>();
    })
    .ConfigureOpenApi()
    .ConfigureLogging(options => options.AddApplicationInsights())
    .ConfigureAppConfiguration(builder => builder.AddConfigurationSettings())
    .ConfigureServices((services) =>
    {
        services.ConfigureServices();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.ConfigureLoggerFilterOptions();

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
