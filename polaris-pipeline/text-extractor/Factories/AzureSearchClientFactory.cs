using System;
using Azure;
using Azure.Core.Serialization;
using Azure.Search.Documents;
using text_extractor.Constants;
using text_extractor.Factories.Contracts;
using Microsoft.Extensions.Configuration;

namespace text_extractor.Factories;

public class AzureSearchClientFactory : IAzureSearchClientFactory
{
    private readonly IConfiguration _configuration;

    public AzureSearchClientFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public SearchClient Create()
    {
        string searchClientEndpointUrl = _configuration[ConfigKeys.SearchClientEndpointUrl] ?? throw new ArgumentNullException(nameof(ConfigKeys.SearchClientEndpointUrl));
        string indexName = _configuration[ConfigKeys.SearchClientIndexName] ?? throw new ArgumentNullException(nameof(ConfigKeys.SearchClientIndexName));
        string searchClientAuthorizationKey = _configuration[ConfigKeys.SearchClientAuthorizationKey] ?? throw new ArgumentNullException(nameof(ConfigKeys.SearchClientAuthorizationKey));

        var sc = new SearchClient(
            new Uri(searchClientEndpointUrl),
            indexName,
            new AzureKeyCredential(searchClientAuthorizationKey),
            new SearchClientOptions { Serializer = new NewtonsoftJsonObjectSerializer() });

        return sc;
    }
}
