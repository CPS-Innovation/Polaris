using Common.Extensions;
using Common.Handlers;
using Common.Telemetry;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using pdf_generator;
using pdf_generator.Services.Extensions;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.ApplicationInsights.Extensibility;
using Common.Middleware;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(options =>
    {
        options.Services.Configure<WorkerOptions>(o =>
        {
            o.EnableUserCodeException = true;
        });

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

        // bugfix: override .net core limitation of disallowing Synchronous IO for this function only
        services.Configure<KestrelServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
            // Kestrel has a default limit of 30MB for request body size. Isolated Azure functions (currently)
            //  have a 100MB default limit which can be controlled by FUNCTIONS_REQUEST_BODY_SIZE_LIMIT
            //  see https://learn.microsoft.com/en-us/azure/azure-functions/functions-app-settings#functions_request_body_size_limit
            //  Let's leave the size limit up to the function-level configuration.
            options.Limits.MaxRequestBodySize = null;
        });

        services.AddSingleton(context.Configuration);
        services.AddPdfGenerator();
        services.AddTransient<IExceptionHandler, ExceptionHandler>();
        services.AddSingleton<ITelemetryClient, TelemetryClient>();
        services.AddSingleton<ITelemetryAugmentationWrapper, TelemetryAugmentationWrapper>();
        services.AddSingleton<Microsoft.ApplicationInsights.TelemetryClient, Microsoft.ApplicationInsights.TelemetryClient>();
    })
    .Build();

host.Run();