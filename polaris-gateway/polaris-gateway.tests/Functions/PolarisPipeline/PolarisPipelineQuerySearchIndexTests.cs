using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Azure;
using Common.Domain.SearchIndex;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using PolarisGateway.Clients.PolarisPipeline;
using PolarisGateway.Domain.Validation;
using PolarisGateway.Domain.Validators;
using PolarisGateway.Functions.PolarisPipeline;
using Xunit;

namespace PolarisGateway.Tests.Functions.PolarisPipeline
{
    public class PolarisPipelineQuerySearchIndexTests : SharedMethods.SharedMethods
    {
        private readonly string _caseUrn;
        private readonly int _caseId;
        private readonly string _searchTerm;
        private readonly Guid _correlationId;

        private readonly Mock<IPipelineClient> _mockPipelineClient;
        private readonly IList<StreamlinedSearchLine> _searchResults;
        private readonly Mock<IAuthorizationValidator> _mockTokenValidator;

        private readonly PolarisPipelineQuerySearchIndex _polarisPipelineQuerySearchIndex;

        public PolarisPipelineQuerySearchIndexTests()
        {
            var fixture = new Fixture();

            _caseUrn = fixture.Create<string>();
            _caseId = fixture.Create<int>();
            _searchTerm = fixture.Create<string>();
            _searchResults = fixture.Create<IList<StreamlinedSearchLine>>();
            _correlationId = fixture.Create<Guid>();

            var mockLogger = new Mock<ILogger<PolarisPipelineQuerySearchIndex>>();

            _mockPipelineClient = new Mock<IPipelineClient>();
            _mockPipelineClient.Setup(x => x.SearchCase(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid>())).ReturnsAsync(_searchResults);
            _mockTokenValidator = new Mock<IAuthorizationValidator>();
            _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new ValidateTokenResult { IsValid = true, UserName = "user-name" });

            _polarisPipelineQuerySearchIndex = new PolarisPipelineQuerySearchIndex(mockLogger.Object, _mockPipelineClient.Object, _mockTokenValidator.Object);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenAccessCorrelationIdIsMissing()
        {
            var response = await _polarisPipelineQuerySearchIndex.Run(CreateHttpRequestWithoutCorrelationId(), _caseUrn, _caseId);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenAccessTokenIsMissing()
        {
            var response = await _polarisPipelineQuerySearchIndex.Run(CreateHttpRequestWithoutToken(), _caseUrn, _caseId);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsUnauthorizedWhenAccessTokenIsInvalid()
        {
            _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new ValidateTokenResult { IsValid = false });
            var response = await _polarisPipelineQuerySearchIndex.Run(CreateHttpRequest(), _caseUrn, _caseId);

            response.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task Run_ReturnsBadRequestWhenCaseId_IsNotAValidValue(int caseId)
        {
            var response = await _polarisPipelineQuerySearchIndex.Run(CreateHttpRequest(), _caseUrn, caseId);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Run_ReturnsBadRequestWhenSearchTermIsInvalid(string searchTerm)
        {
            var request = CreateHttpRequest();
            request.QueryString = searchTerm == null ? new QueryString() : new QueryString($"?query={searchTerm}");

            var response = await _polarisPipelineQuerySearchIndex.Run(request, _caseUrn, _caseId);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsOk()
        {
            var response = await _polarisPipelineQuerySearchIndex.Run(CreateHttpRequest(), _caseUrn, _caseId);

            response.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsSearchResults()
        {
            var response = await _polarisPipelineQuerySearchIndex.Run(CreateHttpRequest(), _caseUrn, _caseId) as OkObjectResult;

            response?.Value.Should().Be(_searchResults);
        }

        [Fact]
        public async Task Run_ReturnsInternalServerErrorWhenRequestFailedExceptionOccurs()
        {
            _mockPipelineClient.Setup(client => client.SearchCase(_caseUrn, _caseId, _searchTerm, _correlationId))
                .ThrowsAsync(new RequestFailedException("Test"));

            var response = await _polarisPipelineQuerySearchIndex.Run(CreateHttpRequest(), _caseUrn, _caseId) as StatusCodeResult;

            response?.StatusCode.Should().Be(500);
        }


        [Fact]
        public async Task Run_ReturnsInternalServerErrorWhenUnhandledExceptionOccurs()
        {
            _mockPipelineClient.Setup(client => client.SearchCase(_caseUrn, _caseId, _searchTerm, _correlationId))
                .ThrowsAsync(new RequestFailedException("Test"));

            var response = await _polarisPipelineQuerySearchIndex.Run(CreateHttpRequest(), _caseUrn, _caseId) as StatusCodeResult;

            response?.StatusCode.Should().Be(500);
        }
    }
}

