using System.IO;
using System.Linq;
using Common.Domain.Validators;
using Common.Dto.Request;
using Common.Handlers;
using Common.Handlers.Contracts;
using Common.Services.Extensions;
using Common.Telemetry;
using Common.Telemetry.Contracts;
using Common.Telemetry.Wrappers;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using pdf_generator;
using pdf_generator.Services.Extensions;
using Common.Telemetry.Wrappers.Contracts;
using Microsoft.AspNetCore.Server.Kestrel.Core;

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

        services.Configure<WorkerOptions>(o =>
        {
            o.EnableUserCodeException = true;
        });

        // bugfix: override .net core limitation of disallowing Synchronous IO for this function only
        services.Configure<KestrelServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
        });

        services.AddSingleton(context.Configuration);
        services.AddPdfGenerator();
        services.AddRedactionServices(context.Configuration);

        services.AddBlobStorageWithDefaultAzureCredential(context.Configuration);
        services.AddScoped<IValidator<RedactPdfRequestDto>, RedactPdfRequestValidator>();
        services.AddTransient<IExceptionHandler, ExceptionHandler>();
        services.AddSingleton<ITelemetryClient, TelemetryClient>();
        services.AddSingleton<ITelemetryAugmentationWrapper, TelemetryAugmentationWrapper>();
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