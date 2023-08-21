using Common.Wrappers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.Services.OcrService;
using Azure.Identity;
using System;
using System.Diagnostics.CodeAnalysis;
using Common.Constants;
using Common.Factories.Contracts;
using Common.Factories;
using Common.Health;
using Common.Services.Extensions;
using Common.Wrappers.Contracts;
using System.IO;
using Common.Dto.Request;
using Common.Handlers.Contracts;
using Common.Handlers;
using Common.Telemetry.Contracts;
using Common.Telemetry;
using Common.Mappers.Contracts;
using Common.Mappers;

[assembly: FunctionsStartup(typeof(text_extractor.Startup))]
namespace text_extractor
{
    [ExcludeFromCodeCoverage]
    internal class Startup : FunctionsStartup
    {
        protected IConfigurationRoot Configuration { get; set; }

        // https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-dependency-injection#customizing-configuration-sources
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();

            var configurationBuilder = builder.ConfigurationBuilder
                .AddEnvironmentVariables()
#if DEBUG
                .SetBasePath(Directory.GetCurrentDirectory())
#endif
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

            Configuration = configurationBuilder.Build();
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var services = builder.Services;

            services.AddSingleton<IConfiguration>(Configuration);
            BuildOcrService(services, Configuration);
            services.AddSearchClient(Configuration);

            services.AddTransient<IExceptionHandler, ExceptionHandler>();
            services.AddTransient<IValidatorWrapper<ExtractTextRequestDto>, ValidatorWrapper<ExtractTextRequestDto>>();
            services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();

            services.AddTransient<IComputerVisionClientFactory, ComputerVisionClientFactory>();
            services.AddSingleton<ITelemetryClient, TelemetryClient>();

            services.AddSingleton<IDtoHttpRequestHeadersMapper, DtoHttpRequestHeadersMapper>();
            services.AddSingleton<ISearchFilterDocumentMapper, SearchFilterDocumentMapper>();
            BuildHealthChecks(services);
        }

        private static void BuildOcrService(IServiceCollection services, IConfigurationRoot configuration)
        {
#if DEBUG
            if (configuration.IsSettingEnabled(DebugSettings.MockOcrService))
            {
                services.AddSingleton<IOcrService, MockOcrService>();
            }
            else
            {
                services.AddSingleton<IOcrService, OcrService>();
            }
#else
            services.AddSingleton<IOcrService, OcrService>();
#endif
        }

        /// <summary>
        /// see https://www.davidguida.net/azure-api-management-healthcheck/ for pattern
        /// Microsoft.Extensions.Diagnostics.HealthChecks Nuget downgraded to lower release to get package to work
        /// </summary>
        /// <param name="builder"></param>
        private static void BuildHealthChecks(IServiceCollection services)
        {
            services.AddHealthChecks()
                .AddCheck<AzureBlobServiceClientHealthCheck>("Azure Blob Service Client")
                .AddCheck<OcrServiceHealthCheck>("OCR Service")
                .AddCheck<AzureComputerVisionClientHealthCheck>("OCR Service / Azure Computer Vision Client")
                .AddCheck<SearchIndexServiceHealthCheck>("Search Index Service")
                .AddCheck<AzureSearchClientHealthCheck>("Search Index Service / Azure Search Client");
        }
    }
}