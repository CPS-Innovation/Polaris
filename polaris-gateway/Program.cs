using System.Net.Http.Headers;
using Common.Telemetry;
using Common.Wrappers;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using PolarisGateway;
using PolarisGateway.Clients.Coordinator;
using PolarisGateway.Handlers;
using PolarisGateway.Mappers;
using PolarisGateway.Validators;

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
        #if DEBUG
            // https://stackoverflow.com/questions/54435551/invalidoperationexception-idx20803-unable-to-obtain-configuration-from-pii
            IdentityModelEventSource.ShowPII = true;
        #endif
        
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
            // Kestrel has a default limit of 30MB for request body size. Isolated Azure functions (currently)
            //  have a 100MB default limit which can be controlled by FUNCTIONS_REQUEST_BODY_SIZE_LIMIT
            //  see https://learn.microsoft.com/en-us/azure/azure-functions/functions-app-settings#functions_request_body_size_limit
            //  Let's leave the size limit up to the function-level configuration.
            options.Limits.MaxRequestBodySize = null;
        });
            
        services.AddSingleton(context.Configuration);
        services.AddSingleton(_ => new ConfigurationManager<OpenIdConnectConfiguration>(
            $"https://sts.windows.net/{Environment.GetEnvironmentVariable(OAuthSettings.TenantId)}/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever()));
        
        services.AddSingleton<IAuthorizationValidator, AuthorizationValidator>();
        services.AddSingleton<IJsonConvertWrapper, JsonConvertWrapper>();

        services.AddHttpClient<ICoordinatorClient, CoordinatorClient>(client =>
        {
            client.BaseAddress = new Uri(StartupHelpers.GetValueFromConfig(context.Configuration, ConfigurationKeys.PipelineCoordinatorBaseUrl));
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
        }).AddPolicyHandler(StartupHelpers.GetRetryPolicy());

        services.AddSingleton<IRedactPdfRequestMapper, RedactPdfRequestMapper>();
        services.AddSingleton<ITelemetryAugmentationWrapper, TelemetryAugmentationWrapper>();
        services.AddSingleton<ITelemetryClient, TelemetryClient>();
        services.AddSingleton<IUnhandledExceptionHandler, UnhandledExceptionHandler>();
        services.AddSingleton<IInitializationHandler, InitializationHandler>();
        services.AddSingleton<IDocumentNoteRequestMapper, DocumentNoteRequestMapper>();
        services.AddSingleton<IModifyDocumentRequestMapper, ModifyDocumentRequestMapper>();
        services.AddTransient<IRequestFactory, RequestFactory>();
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
    
await host.RunAsync();