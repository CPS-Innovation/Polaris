using AutoFixture;
using Azure.Search.Documents;
using Common.Constants;
using Common.Factories;
using Common.Factories.Contracts;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Common.tests.Factories
{
	public class SearchClientFactoryTests
	{
		private readonly string _searchEndpointUrl;
		private readonly string _searchIndexName;

		private readonly IAzureSearchClientFactory _searchClientFactory;

		public SearchClientFactoryTests()
		{
			var fixture = new Fixture();
			_searchEndpointUrl = "https://www.google.co.uk";
			_searchIndexName = fixture.Create<string>();
			var configuration = new Mock<IConfiguration>();

			configuration.Setup(x => x[ConfigKeys.SharedKeys.SearchClientEndpointUrl]).Returns(_searchEndpointUrl);
			configuration.Setup(x => x[ConfigKeys.SharedKeys.SearchClientIndexName]).Returns(_searchIndexName);
			configuration.Setup(x => x[ConfigKeys.SharedKeys.SearchClientAuthorizationKey]).Returns(fixture.Create<string>());
			
			_searchClientFactory = new AzureSearchClientFactory(configuration.Object);
		}

		[Fact]
		public void Create_ReturnsSearchClient()
		{
			var searchClient = _searchClientFactory.Create();

			searchClient.Should().BeOfType<SearchClient>();
		}

		[Fact]
		public void Create_SetsExpectedEndpoint()
		{
			var searchClient = _searchClientFactory.Create();

			searchClient.Endpoint.Should().Be(_searchEndpointUrl);
		}

		[Fact]
		public void Create_SetsExpectedIndexName()
		{
			var searchClient = _searchClientFactory.Create();

			searchClient.IndexName.Should().Be(_searchIndexName);
		}
	}
}

