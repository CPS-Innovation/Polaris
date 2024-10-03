using Azure.Search.Documents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using text_extractor.Factories;
using text_extractor.Factories.Contracts;
using text_extractor.Mappers;
using text_extractor.Mappers.Contracts;
using text_extractor.Services.CaseSearchService;

namespace text_extractor;

public static class StartupHelpers
{
    public static void AddSearchClient(IServiceCollection services, IConfiguration configuration)
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
}