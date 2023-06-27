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
    private readonly IConfiguration _configuration;
    
    public AzureSearchClientFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public SearchClient Create()
    {
        string searchClientEndpointUrl = _configuration[ConfigKeys.SharedKeys.SearchClientEndpointUrl] ?? throw new ArgumentNullException(nameof(ConfigKeys.SharedKeys.SearchClientEndpointUrl));
        string indexName = _configuration[ConfigKeys.SharedKeys.SearchClientIndexName] ?? throw new ArgumentNullException(nameof(ConfigKeys.SharedKeys.SearchClientIndexName));
        string searchClientAuthorizationKey = _configuration[ConfigKeys.SharedKeys.SearchClientAuthorizationKey] ?? throw new ArgumentNullException(nameof(ConfigKeys.SharedKeys.SearchClientAuthorizationKey));

        var sc = new SearchClient(
            new Uri(searchClientEndpointUrl),
            indexName,
            new AzureKeyCredential(searchClientAuthorizationKey),
            new SearchClientOptions { Serializer = new NewtonsoftJsonObjectSerializer() });
        
        return sc;
    }
}
