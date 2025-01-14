using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using pdf_thumbnail_generator.Durable.Providers;
using Common.Extensions;
using Common.Handlers;
using Common.Services.BlobStorage;
using pdf_thumbnail_generator;
using Common.Telemetry;
using pdf_thumbnail_generator.Services.ClearDownService;
using pdf_thumbnail_generator.Services.ThumbnailGenerationService;
using Common.Wrappers;
using Microsoft.ApplicationInsights.Extensibility;
using Common.Middleware;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(options =>
    {
        options.UseMiddleware<ExceptionHandlingMiddleware>();
        options.UseMiddleware<RequestTelemetryMiddleware>();
    })
    .ConfigureHostConfiguration(builder => builder.AddConfigurationSettings())
    .ConfigureServices((context, services) =>
    {
        StartupHelpers.SetAsposeLicence();

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.Configure<TelemetryConfiguration>(telemetryConfiguration =>
        {
            telemetryConfiguration.DefaultTelemetrySink.TelemetryProcessorChainBuilder
                .UseAdaptiveSampling(maxTelemetryItemsPerSecond: 20, excludedTypes: "Request;Exception;Event;Trace");
            telemetryConfiguration.DisableTelemetry = false;
        });
        services.ConfigureLoggerFilterOptions();

        services.AddTransient<IOrchestrationProvider, OrchestrationProvider>();
        services.AddSingleton<IThumbnailGenerationService, ThumbnailGenerationService>();
        services.AddSingleton<IClearDownService, ClearDownService>();

        services.AddSingleton<ITelemetryClient, TelemetryClient>();
        services.AddTransient<IExceptionHandler, ExceptionHandler>();
        services.AddSingleton<IJsonConvertWrapper, JsonConvertWrapper>();
        services.AddSingleton<Microsoft.ApplicationInsights.TelemetryClient, Microsoft.ApplicationInsights.TelemetryClient>();
        services.AddBlobStorageWithDefaultAzureCredential(context.Configuration);
    })
    .Build();

host.Run();
