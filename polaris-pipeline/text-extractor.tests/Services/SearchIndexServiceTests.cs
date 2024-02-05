using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Common.Domain.SearchIndex;
using Common.Dto.Response;
using Microsoft.Extensions.Logging;
using Moq;
using text_extractor.Factories.Contracts;
using text_extractor.Services.CaseSearchService;
using Xunit;

namespace text_extractor.tests.Services
{
    public class SearchIndexServiceTests
    {
        private const long SearchLineTotal = 100;
        private readonly Fixture _fixture;
        private readonly Mock<IAzureSearchClientFactory> _searchClientFactory;
        private readonly Mock<SearchClient> _azureSearchClient;
        private readonly Mock<ISearchLineFactory> _searchLineFactory;
        private readonly Mock<ISearchIndexingBufferedSenderFactory> _searchIndexingBufferedSenderFactory;
        private readonly Mock<IStreamlinedSearchResultFactory> _streamlinedSearchResultFactory;
        private readonly Mock<ILogger<SearchIndexService>> _logger;
        private readonly SearchIndexService _searchIndexService;

        public SearchIndexServiceTests()
        {
            var responseMock = new Mock<Response>();

            _fixture = new Fixture();

            _azureSearchClient = new Mock<SearchClient>(() => new SearchClient(new Uri("https://localhost"), "index", new AzureKeyCredential("key")));
            _azureSearchClient.Setup(x => x.SearchAsync<SearchLineId>(It.IsAny<string>(), It.IsAny<SearchOptions>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(
                        Response.FromValue(
                            SearchModelFactory.SearchResults(new[]
                            {
                                SearchModelFactory.SearchResult(new SearchLineId(), 0.9, null),
                            },
                            SearchLineTotal,
                            null,
                            null,
                            responseMock.Object
                        ),
                        responseMock.Object))
                    );

            _searchClientFactory = new Mock<IAzureSearchClientFactory>();
            _searchClientFactory.Setup(x => x.Create()).Returns(_azureSearchClient.Object);

            _searchLineFactory = new Mock<ISearchLineFactory>();
            _searchIndexingBufferedSenderFactory = new Mock<ISearchIndexingBufferedSenderFactory>();
            _streamlinedSearchResultFactory = new Mock<IStreamlinedSearchResultFactory>();

            _logger = new Mock<ILogger<SearchIndexService>>();

            _searchIndexService = new SearchIndexService(_searchClientFactory.Object, _searchLineFactory.Object, _searchIndexingBufferedSenderFactory.Object, _streamlinedSearchResultFactory.Object, _logger.Object);
        }

        [Fact]
        public void WhenGettingTheIndexCountForACase_AndTheCaseIdIsZero_AnExceptionIsThrown()
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await _searchIndexService.GetCaseIndexCount(0));
        }

        [Fact]
        public async void WhenGettingTheIndexCountForACase_WithAValidCaseId_AResultObjectIsReturned()
        {
            var result = await _searchIndexService.GetCaseIndexCount(1234);

            Assert.True(result.GetType() == typeof(CaseIndexCountResult));
            Assert.Equal(SearchLineTotal, result.LineCount);
        }
    }
}