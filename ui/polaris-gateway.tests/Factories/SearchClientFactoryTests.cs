using AutoFixture;
using Azure.Search.Documents;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using PolarisGateway.Factories;
using Xunit;

namespace PolarisGateway.Tests.Factories
{
	public class SearchClientFactoryTests
	{
        private readonly Domain.PolarisPipeline.SearchClientOptions _searchClientOptions;

        private readonly ISearchClientFactory _searchClientFactory;

		public SearchClientFactoryTests()
		{
            var fixture = new Fixture();
			_searchClientOptions = fixture.Build<Domain.PolarisPipeline.SearchClientOptions>()
									.With(o => o.EndpointUrl, "https://www.google.co.uk")
									.Create();

            var mockSearchClientOptions = new Mock<IOptions<Domain.PolarisPipeline.SearchClientOptions>>();

			mockSearchClientOptions.Setup(options => options.Value).Returns(_searchClientOptions);

			_searchClientFactory = new SearchClientFactory(mockSearchClientOptions.Object);
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

			searchClient.Endpoint.Should().Be(_searchClientOptions.EndpointUrl);
		}

		[Fact]
		public void Create_SetsExpectedIndexName()
		{
			var searchClient = _searchClientFactory.Create();

			searchClient.IndexName.Should().Be(_searchClientOptions.IndexName);
		}
	}
}

