using Common.Extensions;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureHostConfiguration(builder =>
    {
        builder.AddEnvironmentVariables();
#if DEBUG
        builder.SetBasePath(Directory.GetCurrentDirectory());
#endif
        builder.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService(new ApplicationInsightsServiceOptions
        {
            EnableAdaptiveSampling = false,
        });

        services.ConfigureFunctionsApplicationInsights();
        services.ConfigureLoggerFilterOptions();
    })
    .Build();

host.Run();
