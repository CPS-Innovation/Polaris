using System;
using Azure;
using Azure.Core.Serialization;
using Azure.Search.Documents;
using Microsoft.Extensions.Options;

namespace RumpoleGateway.Factories
{
	public class SearchClientFactory : ISearchClientFactory
	{
        private readonly Domain.RumpolePipeline.SearchClientOptions _options;
        
        public SearchClientFactory(IOptions<Domain.RumpolePipeline.SearchClientOptions> options)
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

