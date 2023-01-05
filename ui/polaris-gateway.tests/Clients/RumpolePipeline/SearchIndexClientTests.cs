﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using Moq;
using RumpoleGateway.Clients.RumpolePipeline;
using RumpoleGateway.Domain.RumpolePipeline;
using RumpoleGateway.Factories;
using RumpoleGateway.Mappers;
using Xunit;

namespace RumpoleGateway.Tests.Clients.RumpolePipeline
{
	public class SearchIndexClientTests
	{
		private readonly Fixture _fixture;
		private readonly int _caseId;
		private readonly string _searchTerm;
		private readonly Guid _correlationId;

		private readonly Mock<SearchClient> _mockSearchClient;

		private readonly ISearchIndexClient _searchIndexClient;

		public SearchIndexClientTests()
		{
			_fixture = new Fixture();
			_caseId = _fixture.Create<int>();
			_searchTerm = _fixture.Create<string>();
			_correlationId = _fixture.Create<Guid>();

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
			var results = await _searchIndexClient.Query(_caseId, _searchTerm, _correlationId);

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

		[Fact]
		public async Task Query_ResultsAreOrderedByDocumentId()
		{
			var responseMock = new Mock<Response>();
			var fakeSearchLines = _fixture.CreateMany<SearchLine>(3).ToList();
			fakeSearchLines[0].DocumentId = "XYZ";
			fakeSearchLines[1].DocumentId = "LMN";
			fakeSearchLines[2].DocumentId = "ABC";
			
			_mockSearchClient.Setup(client => client.SearchAsync<SearchLine>(_searchTerm, 
					It.Is<SearchOptions>(o => o.Filter == $"caseId eq {_caseId}"), It.IsAny<CancellationToken>()))
				.Returns(Task.FromResult(
					Response.FromValue(
						SearchModelFactory.SearchResults(new[] {
							SearchModelFactory.SearchResult(fakeSearchLines[2], 0.8, null),
							SearchModelFactory.SearchResult(fakeSearchLines[1], 0.8, null),
							SearchModelFactory.SearchResult(fakeSearchLines[0], 0.9, null)
						}, 100, null, null, responseMock.Object), responseMock.Object)));
			
			var results = await _searchIndexClient.Query(_caseId, _searchTerm, _correlationId);

			using (new AssertionScope())
			{
				results.Count.Should().Be(3);
				results[0].DocumentId.Should().Be("ABC");
				results[1].DocumentId.Should().Be("LMN");
				results[2].DocumentId.Should().Be("XYZ");
			}
		}

        [Fact]
        public async Task Query_GivenValidSearchResults_ThenAStreamlinedResponseIsReturned()
        {
            var responseMock = new Mock<Response>();
            var fakeSearchLines = _fixture.CreateMany<SearchLine>(3).ToList();
            fakeSearchLines[0].DocumentId = "XYZ";
            fakeSearchLines[1].DocumentId = "LMN";
            fakeSearchLines[2].DocumentId = "ABC";

            _mockSearchClient.Setup(client => client.SearchAsync<SearchLine>(_searchTerm,
                    It.Is<SearchOptions>(o => o.Filter == $"caseId eq {_caseId}"), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(
                    Response.FromValue(
                        SearchModelFactory.SearchResults(new[] {
                            SearchModelFactory.SearchResult(fakeSearchLines[2], 0.8, null),
                            SearchModelFactory.SearchResult(fakeSearchLines[1], 0.8, null),
                            SearchModelFactory.SearchResult(fakeSearchLines[0], 0.9, null)
                        }, 100, null, null, responseMock.Object), responseMock.Object)));

            var streamlinedResults = await _searchIndexClient.Query(_caseId, _searchTerm, _correlationId);

            using (new AssertionScope())
            {
                streamlinedResults.Count.Should().Be(fakeSearchLines.Count);
                streamlinedResults[0].DocumentId.Should().Be(fakeSearchLines[2].DocumentId);
                streamlinedResults[0].Words.Count.Should().Be(fakeSearchLines[2].Words.Count);
            }
		}
        
        [Fact]
        public async Task Query_GivenValidSearchResults_BoundingBoxShouldBeNull_IfSearchTermIsNotFoundInTheWordText()
        {
	        var responseMock = new Mock<Response>();
	        var fakeSearchLines = _fixture.CreateMany<SearchLine>(3).ToList();
	        fakeSearchLines[0].DocumentId = "XYZ";
	        fakeSearchLines[1].DocumentId = "LMN";
	        fakeSearchLines[2].DocumentId = "ABC";

	        fakeSearchLines[2].Words = _fixture.CreateMany<Word>(2).ToList();
	        fakeSearchLines[2].Words[0].Text = string.Join(" ", _searchTerm);
	        fakeSearchLines[2].Words[1].Text = string.Join(" ", Guid.NewGuid().ToString());

	        _mockSearchClient.Setup(client => client.SearchAsync<SearchLine>(_searchTerm,
			        It.Is<SearchOptions>(o => o.Filter == $"caseId eq {_caseId}"), It.IsAny<CancellationToken>()))
		        .Returns(Task.FromResult(
			        Response.FromValue(
				        SearchModelFactory.SearchResults(new[] {
					        SearchModelFactory.SearchResult(fakeSearchLines[2], 0.8, null),
					        SearchModelFactory.SearchResult(fakeSearchLines[1], 0.8, null),
					        SearchModelFactory.SearchResult(fakeSearchLines[0], 0.9, null)
				        }, 100, null, null, responseMock.Object), responseMock.Object)));

	        var streamlinedResults = await _searchIndexClient.Query(_caseId, _searchTerm, _correlationId);

	        using (new AssertionScope())
	        {
		        streamlinedResults.Count.Should().Be(fakeSearchLines.Count);
		        streamlinedResults[0].DocumentId.Should().Be(fakeSearchLines[2].DocumentId);
		        streamlinedResults[0].Words.Count.Should().Be(fakeSearchLines[2].Words.Count);
		        streamlinedResults[0].Words[0].BoundingBox.Should().NotBeNull();
		        streamlinedResults[0].Words[1].BoundingBox.Should().BeNull();
	        }
        }
	}
}

