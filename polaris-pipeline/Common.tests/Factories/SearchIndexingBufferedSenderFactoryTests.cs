using Azure.Search.Documents;
using Common.Domain.SearchIndex;
using Common.Factories;
using FluentAssertions;
using Moq;
using Xunit;

namespace Common.tests.Factories
{
	public class SearchIndexingBufferedSenderFactoryTests
	{
		[Fact]
		public void Create_ReturnsSearchIndexBufferedSender()
        {
			var searchClient = new Mock<SearchClient>();
			var factory = new SearchIndexingBufferedSenderFactory();

			var sender = factory.Create(searchClient.Object);

			sender.Should().BeOfType<SearchIndexingBufferedSender<SearchLine>>();
        }
	}
}

