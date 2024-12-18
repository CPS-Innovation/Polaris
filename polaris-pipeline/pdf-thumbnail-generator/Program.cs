using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using pdf_thumbnail_generator.Durable.Providers;
using Microsoft.Extensions.Configuration;
using Common.Handlers;
using Common.Services.BlobStorage;
using Microsoft.Extensions.Logging;
using pdf_thumbnail_generator;
using Common.Telemetry;
using pdf_thumbnail_generator.Services.ClearDownService;
using pdf_thumbnail_generator.Services.ThumbnailGenerationService;
using Common.Wrappers;

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
    .ConfigureServices((context, services) =>
    {
        StartupHelpers.SetAsposeLicence();

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddTransient<IOrchestrationProvider, OrchestrationProvider>();
        services.AddSingleton<IThumbnailGenerationService, ThumbnailGenerationService>();
        services.AddSingleton<IClearDownService, ClearDownService>();

        services.AddSingleton<ITelemetryClient, TelemetryClient>();
        services.AddTransient<IExceptionHandler, ExceptionHandler>();
        services.AddSingleton<IJsonConvertWrapper, JsonConvertWrapper>();
        services.AddBlobStorageWithDefaultAzureCredential(context.Configuration);
    })
    .ConfigureLogging(logging =>
    {
        // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs. Application Insights requires an explicit override.
        // Log levels can also be configured using appsettings.json. For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
        logging.Services.Configure<LoggerFilterOptions>(options =>
        {
            var defaultRule = options.Rules.FirstOrDefault(rule =>
                rule.ProviderName ==
                "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
            if (defaultRule is not null)
            {
                options.Rules.Remove(defaultRule);
            }
        });
    })
    .Build();

host.Run();
