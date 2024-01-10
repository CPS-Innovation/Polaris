using Azure;
using Azure.Core.Serialization;
using Azure.Search.Documents;
using Microsoft.Extensions.Configuration;
using polaris_common.Constants;
using polaris_common.Factories.Contracts;

namespace polaris_common.Factories;

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
