using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Common.Domain.Validators;
using Common.Dto.Request;
using Common.Extensions;
using Common.Handlers;
using Common.Telemetry;
using Common.Wrappers;
using FluentValidation;
using pdf_redactor;
using pdf_redactor.Services.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Common.Middleware;
using Microsoft.ApplicationInsights.WorkerService;

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
        services.AddRedactionServices(context.Configuration);
        services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();

        services.AddScoped<IValidator<RedactPdfRequestDto>, RedactPdfRequestValidator>();
        services.AddScoped<IValidator<RedactPdfRequestWithDocumentDto>, RedactPdfRequestWithDocumentValidator>();
        services.AddScoped<IValidator<ModifyDocumentWithDocumentDto>, ModifyDocumentWithDocumentValidator>();
        services.AddTransient<IExceptionHandler, ExceptionHandler>();
        services.AddSingleton<ITelemetryClient, TelemetryClient>();
        services.AddSingleton<Microsoft.ApplicationInsights.TelemetryClient, Microsoft.ApplicationInsights.TelemetryClient>();
    })
    .Build();

host.Run();