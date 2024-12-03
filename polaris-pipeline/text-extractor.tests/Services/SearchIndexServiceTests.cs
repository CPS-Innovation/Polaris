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
using text_extractor.Mappers.Contracts;
using text_extractor.Services.SearchIndexService;
using Xunit;

namespace text_extractor.tests.Services
{
    public class SearchIndexServiceTests
    {
        private const long SearchLineTotal = 100;
        private const int ResultCaseId = 1234;
        private const int NoResultCaseId = 9999;
        private readonly Guid _correlationId;
        private readonly SearchIndexService _searchIndexService;

        public SearchIndexServiceTests()
        {
            var responseMock = new Mock<Response>();

            var fixture = new Fixture();
            _correlationId = fixture.Create<Guid>();

            var azureSearchClient = new Mock<SearchClient>(() => new SearchClient(new Uri("https://localhost"), "index", new AzureKeyCredential("key")));
            azureSearchClient.Setup(x => x.SearchAsync<SearchLineId>(It.IsAny<string>(), It.IsAny<SearchOptions>(), It.IsAny<CancellationToken>()))
                .Returns(
                    Task.FromResult(
                        Response.FromValue(
                            SearchModelFactory.SearchResults(new[]
                            {
                                SearchModelFactory.SearchResult(new SearchLineId { Id = "ABC123" }, 0.9, null),
                            },
                            SearchLineTotal,
                            null,
                            null,
                            responseMock.Object
                        ),
                        responseMock.Object))
                    );
            azureSearchClient.Setup(x => x.SearchAsync<SearchLineId>(It.IsAny<string>(), It.Is<SearchOptions>(s => s.Filter == $"caseId eq {NoResultCaseId}"), default))
                .Returns(
                    Task.FromResult(
                        Response.FromValue(
                            SearchModelFactory.SearchResults([
                                    SearchModelFactory.SearchResult(new SearchLineId(), 0.9, null)
                                ],
                            0,
                            null,
                            null,
                            responseMock.Object
                        ),
                        responseMock.Object))
                    );


            var searchClientFactory = new Mock<IAzureSearchClientFactory>();
            searchClientFactory.Setup(x => x.Create()).Returns(azureSearchClient.Object);

            var searchLineFactory = new Mock<ISearchLineFactory>();
            var searchIndexingBufferedSender = new Mock<SearchIndexingBufferedSender<ISearchable>>();
            var searchIndexingBufferedSenderFactory = new Mock<ISearchIndexingBufferedSenderFactory>();
            searchIndexingBufferedSenderFactory.Setup(x => x.Create(azureSearchClient.Object)).Returns(searchIndexingBufferedSender.Object);
            var streamlinedSearchResultFactory = new Mock<IStreamlinedSearchResultFactory>();
            var lineMapper = new Mock<ILineMapper>();

            var logger = new Mock<ILogger<SearchIndexService>>();

            _searchIndexService = new SearchIndexService(searchClientFactory.Object, searchLineFactory.Object, searchIndexingBufferedSenderFactory.Object, streamlinedSearchResultFactory.Object, lineMapper.Object, logger.Object);
        }

        [Fact]
        public async Task WhenGettingTheIndexCountForACase_AndTheCaseIdIsZero_AnExceptionIsThrown()
        {
            await Assert.ThrowsAsync<ArgumentException>(async () => await _searchIndexService.GetCaseIndexCount(0, _correlationId));
        }

        [Fact]
        public async Task WhenGettingTheIndexCountForACase_WithAValidCaseId_AResultObjectIsReturned()
        {
            var result = await _searchIndexService.GetCaseIndexCount(ResultCaseId, _correlationId);

            Assert.True(result.GetType() == typeof(SearchIndexCountResult));
            Assert.Equal(SearchLineTotal, result.LineCount);
        }

        [Fact]
        public async Task WhenGettingTheIndexCountForADocument_AndTheCaseIdIsZero_AnExceptionIsThrown()
        {
            await Assert.ThrowsAsync<ArgumentException>(async () => await _searchIndexService.GetDocumentIndexCount(0, "DOC1", 1, _correlationId));
        }

        [Fact]
        public async Task WhenGettingTheIndexCountForADocument_WithAValidCaseId_AResultObjectIsReturned()
        {
            var result = await _searchIndexService.GetDocumentIndexCount(ResultCaseId, "DOC1", 1, _correlationId);

            Assert.True(result.GetType() == typeof(SearchIndexCountResult));
            Assert.Equal(SearchLineTotal, result.LineCount);
        }

        [Fact]
        public async Task WhenRemovingCaseIndexEntries_AndTheCaseIdIsZero_AnExceptionIsThrown()
        {
            await Assert.ThrowsAsync<ArgumentException>(async () => await _searchIndexService.RemoveCaseIndexEntriesAsync(0, _correlationId));
        }

        [Fact]
        public async Task WhenRemovingCaseIndexEntries_AndNoResultsAreReturned_AnEmptyIndexDocumentsDeletedResultIsReturned()
        {
            var result = await _searchIndexService.RemoveCaseIndexEntriesAsync(NoResultCaseId, _correlationId);

            Assert.True(result.GetType() == typeof(IndexDocumentsDeletedResult));
            Assert.Equal(0, result.SuccessCount);
            Assert.Equal(0, result.FailureCount);
            Assert.Equal(0, result.DocumentCount);
        }
    }
}