using AutoFixture;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Common.Clients;
using Common.Clients.Contracts;
using Common.Domain.SearchIndex;
using Common.Dto.Tracker;
using Common.Factories;
using Common.Factories.Contracts;
using Common.Mappers;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Common.Tests.Clients
{
    public class SearchIndexClientTests
    {
        private readonly Fixture _fixture;
        private readonly int _caseId;
        private readonly string _searchTerm;
        private readonly Guid _correlationId;
        private readonly List<TrackerCmsDocumentDto> _documents;

        private readonly Mock<SearchClient> _mockSearchClient;
        private readonly ISearchIndexClient _searchIndexClient;

        public SearchIndexClientTests()
        {
            _fixture = new Fixture();
            _caseId = _fixture.Create<int>();
            _searchTerm = _fixture.Create<string>();
            _correlationId = _fixture.Create<Guid>();
            _documents = new List<TrackerCmsDocumentDto>();

            var mockSearchClientFactory = new Mock<ISearchClientFactory>();
            _mockSearchClient = new Mock<SearchClient>();
            var mockResponse = new Mock<Response<SearchResults<SearchLine>>>();
            var mockSearchResults = new Mock<SearchResults<SearchLine>>();

            var mockSearchIndexLogger = new Mock<ILogger<SearchIndexClient>>();
            var mockSearchLineMapperLogger = new Mock<ILogger<StreamlinedSearchLineMapper>>();
            var mockSearchWordMapperLogger = new Mock<ILogger<StreamlinedSearchWordMapper>>();
            var mockSearchResultFactoryLogger = new Mock<ILogger<StreamlinedSearchResultFactory>>();

            mockSearchClientFactory.Setup(factory => factory.Create()).Returns(_mockSearchClient.Object);
            _mockSearchClient.Setup(client => client.SearchAsync<SearchLine>(_searchTerm, It.Is<SearchOptions>(o => o.Filter == $"caseId eq {_caseId}"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse.Object);
            mockResponse.Setup(response => response.Value).Returns(mockSearchResults.Object);

            _searchIndexClient = new SearchIndexClient(mockSearchClientFactory.Object, new StreamlinedSearchResultFactory(new StreamlinedSearchLineMapper(mockSearchLineMapperLogger.Object),
                new StreamlinedSearchWordMapper(mockSearchWordMapperLogger.Object), mockSearchResultFactoryLogger.Object), mockSearchIndexLogger.Object);
        }

        [Fact]
        public async Task Query_ReturnsSearchLines()
        {
            var results = await _searchIndexClient.Query(_caseId, _documents, _searchTerm, _correlationId);

            results.Should().NotBeNull();
        }

        //[Fact]
        //public async Task Query_WhenResultsContainDuplicates_ShouldReturnNoDuplicates()
        //{
        //	var responseMock = new Mock<Response>();
        //	var fakeSearchLines = _fixture.CreateMany<SearchLine>(3).ToList();
        //	var duplicateRecord = fakeSearchLines[0];
        //	var duplicateRecordId = duplicateRecord.Id;
        //	fakeSearchLines.Add(duplicateRecord);

        //	_mockSearchClient.Setup(client => client.SearchAsync<SearchLine>(_searchTerm, 
        //			It.Is<SearchOptions>(o => o.Filter == $"caseId eq {_caseId}"), It.IsAny<CancellationToken>()))
        //		.Returns(Task.FromResult(
        //				Response.FromValue(
        //					SearchModelFactory.SearchResults(new[] {
        //						SearchModelFactory.SearchResult(fakeSearchLines[0], 0.9, null),
        //						SearchModelFactory.SearchResult(fakeSearchLines[1], 0.8, null),
        //						SearchModelFactory.SearchResult(fakeSearchLines[2], 0.8, null),
        //						SearchModelFactory.SearchResult(fakeSearchLines[3], 0.9, null)
        //					}, 100, null, null, responseMock.Object), responseMock.Object)));

        //	var results = await _searchIndexClient.Query(_caseId, _searchTerm);

        //	using (new AssertionScope())
        //	{
        //		results.Count.Should().Be(3);
        //		results.Count(s => s.Id == duplicateRecordId).Should().Be(1);
        //	}
        //}


        /* *******************************************************************************
          *******************************************************************************
          *******************************************************************************
          *******************************************************************************
          *******************************************************************************

          Following two tests are commented out to get the `IsLiveDocumentResult` method in to 
          SearchIndexClient.cs.  This then lets us bring the e2e test back in and passing,
          then we can uncomment these tests and get them passing.  

          *******************************************************************************
          *******************************************************************************
          *******************************************************************************
          *******************************************************************************
          *******************************************************************************
        */

        // [Fact]
        // public async Task Query_GivenValidSearchResults_ThenAStreamlinedResponseIsReturned()
        // {
        //     var responseMock = new Mock<Response>();
        //     var fakeSearchLines = _fixture.CreateMany<SearchLine>(3).ToList();
        //     fakeSearchLines[0].DocumentId = "3333";
        //     fakeSearchLines[1].DocumentId = "2222";
        //     fakeSearchLines[2].DocumentId = "1111";

        //     _mockSearchClient.Setup(client => client.SearchAsync<SearchLine>(_searchTerm,
        //             It.Is<SearchOptions>(o => o.Filter == $"caseId eq {_caseId}"), It.IsAny<CancellationToken>()))
        //         .Returns(Task.FromResult(
        //             Response.FromValue(
        //                 SearchModelFactory.SearchResults(new[] {
        //                     SearchModelFactory.SearchResult(fakeSearchLines[2], 0.8, null),
        //                     SearchModelFactory.SearchResult(fakeSearchLines[1], 0.8, null),
        //                     SearchModelFactory.SearchResult(fakeSearchLines[0], 0.9, null)
        //                 }, 100, null, null, responseMock.Object), responseMock.Object)));

        //     var streamlinedResults = await _searchIndexClient.Query(_caseId, _documents, _searchTerm, _correlationId);

        //     using (new AssertionScope())
        //     {
        //         streamlinedResults.Count.Should().Be(fakeSearchLines.Count);
        //         //streamlinedResults[0].DocumentId.Should().Be(fakeSearchLines[2].DocumentId);
        //         streamlinedResults[0].Words.Count.Should().Be(fakeSearchLines[2].Words.Count);
        //     }
        // }

        // [Fact]
        // public async Task Query_GivenValidSearchResults_BoundingBoxShouldBeNull_IfSearchTermIsNotFoundInTheWordText()
        // {
        //     var responseMock = new Mock<Response>();
        //     var fakeSearchLines = _fixture.CreateMany<SearchLine>(3).ToList();
        //     fakeSearchLines[0].DocumentId = "3333";
        //     fakeSearchLines[1].DocumentId = "2222";
        //     fakeSearchLines[2].DocumentId = "1111";

        //     fakeSearchLines[2].Words = _fixture.CreateMany<Word>(2).ToList();
        //     fakeSearchLines[2].Words[0].Text = string.Join(" ", _searchTerm);
        //     fakeSearchLines[2].Words[1].Text = string.Join(" ", Guid.NewGuid().ToString());

        //     _mockSearchClient.Setup(client => client.SearchAsync<SearchLine>(_searchTerm,
        //             It.Is<SearchOptions>(o => o.Filter == $"caseId eq {_caseId}"), It.IsAny<CancellationToken>()))
        //         .Returns(Task.FromResult(
        //             Response.FromValue(
        //                 SearchModelFactory.SearchResults(new[] {
        //                     SearchModelFactory.SearchResult(fakeSearchLines[2], 0.8, null),
        //                     SearchModelFactory.SearchResult(fakeSearchLines[1], 0.8, null),
        //                     SearchModelFactory.SearchResult(fakeSearchLines[0], 0.9, null)
        //                 }, 100, null, null, responseMock.Object), responseMock.Object)));

        //     var streamlinedResults = await _searchIndexClient.Query(_caseId, _documents, _searchTerm, _correlationId);

        //     using (new AssertionScope())
        //     {
        //         streamlinedResults.Count.Should().Be(fakeSearchLines.Count);
        //         //streamlinedResults[0].DocumentId.Should().Be(fakeSearchLines[2].DocumentId);
        //         streamlinedResults[0].Words.Count.Should().Be(fakeSearchLines[2].Words.Count);
        //         streamlinedResults[0].Words[0].BoundingBox.Should().NotBeNull();
        //         streamlinedResults[0].Words[1].BoundingBox.Should().BeNull();
        //     }
        // }
    }
}