using System;
using Azure;
using Azure.Core.Serialization;
using Azure.Search.Documents;
using Microsoft.Extensions.Options;

namespace PolarisGateway.Factories
{
	public class SearchClientFactory : ISearchClientFactory
	{
        private readonly Domain.PolarisPipeline.SearchClientOptions _options;
        
        public SearchClientFactory(IOptions<Domain.PolarisPipeline.SearchClientOptions> options)
        {
            _options = options.Value;
        }

		public SearchClient Create()
        {
            var sc = new SearchClient(
                new Uri(_options.EndpointUrl),
                _options.IndexName,
                new AzureKeyCredential(_options.AuthorizationKey),
                new SearchClientOptions { Serializer = new NewtonsoftJsonObjectSerializer() });
            
            return sc;
        }
	}
}

