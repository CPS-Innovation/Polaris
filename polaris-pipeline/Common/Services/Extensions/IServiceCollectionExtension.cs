using Azure.Search.Documents;
using Common.Factories;
using Common.Factories.Contracts;
using Common.Mappers;
using Common.Mappers.Contracts;
using Common.Services.SasGeneratorService;
using Common.Services.CaseSearchService;
using Common.Wrappers;
using Common.Wrappers.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.Services.CaseSearchService.Contracts;

namespace Common.Services.Extensions
{
    public static class IServiceCollectionExtension
    {
        public static void AddBlobSasGenerator(this IServiceCollection services)
        {
            services.AddTransient<IBlobSasBuilderWrapper, BlobSasBuilderWrapper>();
            services.AddTransient<IBlobSasBuilderFactory, BlobSasBuilderFactory>();
            services.AddTransient<IBlobSasBuilderWrapperFactory, BlobSasBuilderWrapperFactory>();
            services.AddTransient<ISasGeneratorService, SasGeneratorService.SasGeneratorService>();
        }

        public static void AddSearchClient(this IServiceCollection services, IConfigurationRoot configuration)
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
            services.AddTransient<ISearchLineFactory, SearchLineFactory>();
            services.AddTransient<ISearchIndexingBufferedSenderFactory, SearchIndexingBufferedSenderFactory>();
        }
    }
}