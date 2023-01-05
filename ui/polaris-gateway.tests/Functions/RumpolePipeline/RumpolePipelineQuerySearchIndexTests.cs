﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Azure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using RumpoleGateway.Clients.RumpolePipeline;
using RumpoleGateway.Domain.RumpolePipeline;
using RumpoleGateway.Domain.Validators;
using RumpoleGateway.Functions.RumpolePipeline;
using Xunit;

namespace RumpoleGateway.Tests.Functions.RumpolePipeline
{
	public class RumpolePipelineQuerySearchIndexTests : SharedMethods.SharedMethods
	{
        private readonly int _caseId;
		private readonly string _searchTerm;
		private readonly Guid _correlationId;
		private readonly IList<StreamlinedSearchLine> _searchResults;

        private readonly Mock<ISearchIndexClient> _searchIndexClient;
        private readonly Mock<IAuthorizationValidator> _mockTokenValidator;

		private readonly RumpolePipelineQuerySearchIndex _rumpolePipelineQuerySearchIndex;

		public RumpolePipelineQuerySearchIndexTests()
		{
            var fixture = new Fixture();
			_caseId = fixture.Create<int>();
			_searchTerm = fixture.Create<string>();
			_searchResults = fixture.Create<IList<StreamlinedSearchLine>>();
			_correlationId = fixture.Create<Guid>();

			var mockLogger = new Mock<ILogger<RumpolePipelineQuerySearchIndex>>();
			_searchIndexClient = new Mock<ISearchIndexClient>();

			_searchIndexClient.Setup(client => client.Query(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid>()))
				.ReturnsAsync(_searchResults);

            _mockTokenValidator = new Mock<IAuthorizationValidator>();

            _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            _rumpolePipelineQuerySearchIndex = new RumpolePipelineQuerySearchIndex(mockLogger.Object, _searchIndexClient.Object, _mockTokenValidator.Object);
		}
		
		[Fact]
		public async Task Run_ReturnsBadRequestWhenAccessCorrelationIdIsMissing()
		{
			var response = await _rumpolePipelineQuerySearchIndex.Run(CreateHttpRequestWithoutCorrelationId(), _caseId, _searchTerm);

			response.Should().BeOfType<BadRequestObjectResult>();
		}

		[Fact]
		public async Task Run_ReturnsBadRequestWhenAccessTokenIsMissing()
        {
			var response = await _rumpolePipelineQuerySearchIndex.Run(CreateHttpRequestWithoutToken(), _caseId, _searchTerm);

			response.Should().BeOfType<BadRequestObjectResult>();
        }

		[Fact]
		public async Task Run_ReturnsUnauthorizedWhenAccessTokenIsInvalid()
		{
			_mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
			var response = await _rumpolePipelineQuerySearchIndex.Run(CreateHttpRequest(), _caseId, _searchTerm);

			response.Should().BeOfType<UnauthorizedObjectResult>();
		}
		
		[Theory]
		[InlineData(-1)]
		[InlineData(0)]
		public async Task Run_ReturnsBadRequestWhenCaseId_IsNotAValidValue(int caseId)
		{
			var response = await _rumpolePipelineQuerySearchIndex.Run(CreateHttpRequest(), caseId, _searchTerm);

			response.Should().BeOfType<BadRequestObjectResult>();
		}

		[Theory]
        [InlineData(null)]
		[InlineData("")]
		[InlineData(" ")]
		public async Task Run_ReturnsBadRequestWhenSearchTermIsInvalid(string searchTerm)
		{
			var response = await _rumpolePipelineQuerySearchIndex.Run(CreateHttpRequest(), _caseId, searchTerm);

			response.Should().BeOfType<BadRequestObjectResult>();
		}

		[Fact]
		public async Task Run_ReturnsOk()
		{
			var response = await _rumpolePipelineQuerySearchIndex.Run(CreateHttpRequest(), _caseId, _searchTerm);

			response.Should().BeOfType<OkObjectResult>();
		}

		[Fact]
		public async Task Run_ReturnsSearchResults()
		{
			var response = await _rumpolePipelineQuerySearchIndex.Run(CreateHttpRequest(), _caseId, _searchTerm) as OkObjectResult;

			response?.Value.Should().Be(_searchResults);
		}

		[Fact]
		public async Task Run_ReturnsInternalServerErrorWhenRequestFailedExceptionOccurs()
        {
			_searchIndexClient.Setup(client => client.Query(_caseId, _searchTerm, _correlationId))
				.ThrowsAsync(new RequestFailedException("Test"));

			var response = await _rumpolePipelineQuerySearchIndex.Run(CreateHttpRequest(), _caseId, _searchTerm) as StatusCodeResult;

			response?.StatusCode.Should().Be(500);
		}


		[Fact]
		public async Task Run_ReturnsInternalServerErrorWhenUnhandledExceptionOccurs()
		{
			_searchIndexClient.Setup(client => client.Query(_caseId, _searchTerm, _correlationId))
				.ThrowsAsync(new RequestFailedException("Test"));

			var response = await _rumpolePipelineQuerySearchIndex.Run(CreateHttpRequest(), _caseId, _searchTerm) as StatusCodeResult;

			response?.StatusCode.Should().Be(500);
		}
	}
}

