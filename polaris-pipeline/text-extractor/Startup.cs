using Common.Wrappers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using text_extractor.Services.OcrService;
using text_extractor.Factories;
using text_extractor.Factories.Contracts;
using text_extractor.Services.CaseSearchService;
using text_extractor.Mappers.Contracts;
using text_extractor.Mappers;
using System.Diagnostics.CodeAnalysis;
using Azure.Search.Documents;
using Common.Dto.Request;
using Common.Handlers;
using Common.Mappers;
using Common.Telemetry;
using System.IO;

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
            AddSearchClient(services, Configuration);

            services.AddTransient<IExceptionHandler, ExceptionHandler>();
            services.AddTransient<IValidatorWrapper<ExtractTextRequestDto>, ValidatorWrapper<ExtractTextRequestDto>>();
            services.AddTransient<IJsonConvertWrapper, JsonConvertWrapper>();
            services.AddTransient<IComputerVisionClientFactory, ComputerVisionClientFactory>();
            services.AddSingleton<ITelemetryClient, TelemetryClient>();
            services.AddSingleton<ITelemetryAugmentationWrapper, TelemetryAugmentationWrapper>();
            services.AddSingleton<IDtoHttpRequestHeadersMapper, DtoHttpRequestHeadersMapper>();
            services.AddSingleton<ISearchFilterDocumentMapper, SearchFilterDocumentMapper>();
        }

        private static void AddSearchClient(IServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddOptions<SearchClientOptions>().Configure<IConfiguration>((settings, _) =>
            {
                configuration.GetSection("searchClient").Bind(settings);
            });

            services.AddTransient<ISearchIndexService, SearchIndexService>();
            services.AddTransient<IAzureSearchClientFactory, AzureSearchClientFactory>();
            services.AddTransient<IStreamlinedSearchResultFactory, StreamlinedSearchResultFactory>();
            services.AddTransient<IStreamlinedSearchLineMapper, StreamlinedSearchLineMapper>();
            services.AddTransient<IStreamlinedSearchWordMapper, StreamlinedSearchWordMapper>();
            services.AddTransient<ILineMapper, LineMapper>();
            services.AddTransient<ISearchLineFactory, SearchLineFactory>();
            services.AddTransient<ISearchIndexingBufferedSenderFactory, SearchIndexingBufferedSenderFactory>();
        }

        private static void BuildOcrService(IServiceCollection services, IConfigurationRoot configuration)
        {
#if DEBUG
            if (configuration.IsSettingEnabled(MockOcrService.MockOcrServiceSetting))
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
    }
}