using Common.Dto.Request;
using Common.Extensions;
using Common.Handlers;
using Common.Middleware;
using Common.Telemetry;
using Common.Wrappers;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using text_extractor;
using text_extractor.Mappers;
using text_extractor.Mappers.Contracts;


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
        StartupHelpers.AddSearchClient(services, context.Configuration);
        
        services.AddTransient<IExceptionHandler, ExceptionHandler>();
        services.AddTransient<IValidatorWrapper<StoreCaseIndexesRequestDto>, ValidatorWrapper<StoreCaseIndexesRequestDto>>();
        services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
        services.AddSingleton<ITelemetryClient, TelemetryClient>();
        services.AddSingleton<ITelemetryAugmentationWrapper, TelemetryAugmentationWrapper>();
        services.AddSingleton<IDtoHttpRequestHeadersMapper, DtoHttpRequestHeadersMapper>();
        services.AddSingleton<ISearchFilterDocumentMapper, SearchFilterDocumentMapper>();
        services.AddSingleton<Microsoft.ApplicationInsights.TelemetryClient, Microsoft.ApplicationInsights.TelemetryClient>();
    })
    .Build();
    
await host.RunAsync(); 