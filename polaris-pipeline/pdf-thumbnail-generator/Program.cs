using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using pdf_thumbnail_generator.Durable.Providers;
using Microsoft.Extensions.Configuration;
using Common.Extensions;
using Common.Handlers;
using Common.Services.BlobStorage;
using Microsoft.Extensions.Logging;
using pdf_thumbnail_generator;
using Common.Telemetry;
using pdf_thumbnail_generator.Services.ClearDownService;
using pdf_thumbnail_generator.Services.ThumbnailGenerationService;
using Common.Wrappers;
using Microsoft.ApplicationInsights.WorkerService;

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

        services.AddApplicationInsightsTelemetryWorkerService(new ApplicationInsightsServiceOptions
        {
            EnableAdaptiveSampling = false,
        });

        services.ConfigureFunctionsApplicationInsights();
        services.ConfigureLoggerFilterOptions();


        services.AddTransient<IOrchestrationProvider, OrchestrationProvider>();
        services.AddSingleton<IThumbnailGenerationService, ThumbnailGenerationService>();
        services.AddSingleton<IClearDownService, ClearDownService>();

        services.AddSingleton<ITelemetryClient, TelemetryClient>();
        services.AddTransient<IExceptionHandler, ExceptionHandler>();
        services.AddSingleton<IJsonConvertWrapper, JsonConvertWrapper>();
        services.AddBlobStorageWithDefaultAzureCredential(context.Configuration);
    })
    .Build();

host.Run();
