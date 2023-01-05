using System;
using Azure;
using Azure.Core.Serialization;
using Azure.Search.Documents;
using Common.Constants;
using Common.Factories.Contracts;
using Microsoft.Extensions.Configuration;

namespace Common.Factories;

public class SearchClientFactory : ISearchClientFactory
{
    private readonly IConfiguration _configuration;
    
    public SearchClientFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public SearchClient Create()
    {
        var sc = new SearchClient(
            new Uri(_configuration[ConfigKeys.SharedKeys.SearchClientEndpointUrl]),
            _configuration[ConfigKeys.SharedKeys.SearchClientIndexName],
            new AzureKeyCredential(_configuration[ConfigKeys.SharedKeys.SearchClientAuthorizationKey]),
            new SearchClientOptions { Serializer = new NewtonsoftJsonObjectSerializer() });
        
        return sc;
    }
}
