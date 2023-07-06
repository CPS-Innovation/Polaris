using System;
using Azure;
using Azure.Core.Serialization;
using Azure.Search.Documents;
using Common.Constants;
using Common.Factories.Contracts;
using Microsoft.Extensions.Configuration;

namespace Common.Factories;

public class AzureSearchClientFactory : IAzureSearchClientFactory
{
    // Singleton important: https://devblogs.microsoft.com/azure-sdk/lifetime-management-and-thread-safety-guarantees-of-azure-sdk-net-clients/
    private readonly SearchClient _searchClient;

    public AzureSearchClientFactory(IConfiguration configuration)
    {

        var searchClientEndpointUrl = configuration[ConfigKeys.SharedKeys.SearchClientEndpointUrl] ?? throw new ArgumentNullException(nameof(ConfigKeys.SharedKeys.SearchClientEndpointUrl));
        var indexName = configuration[ConfigKeys.SharedKeys.SearchClientIndexName] ?? throw new ArgumentNullException(nameof(ConfigKeys.SharedKeys.SearchClientIndexName));
        var searchClientAuthorizationKey = configuration[ConfigKeys.SharedKeys.SearchClientAuthorizationKey] ?? throw new ArgumentNullException(nameof(ConfigKeys.SharedKeys.SearchClientAuthorizationKey));

        _searchClient = new SearchClient(
            new Uri(searchClientEndpointUrl),
            indexName,
            new AzureKeyCredential(searchClientAuthorizationKey),
            new SearchClientOptions { Serializer = new NewtonsoftJsonObjectSerializer() });
    }

    public SearchClient Create()
    {
        return _searchClient;
    }
}
