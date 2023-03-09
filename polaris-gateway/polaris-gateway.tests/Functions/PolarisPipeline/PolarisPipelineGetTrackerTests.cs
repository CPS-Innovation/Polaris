using System;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using PolarisGateway.Clients.PolarisPipeline;
using PolarisGateway.Domain.PolarisPipeline;
using PolarisGateway.Domain.Validation;
using PolarisGateway.Domain.Validators;
using PolarisGateway.Functions.PolarisPipeline;
using Xunit;

namespace PolarisGateway.Tests.Functions.PolarisPipeline
{
    public class PolarisPipelineGetTrackerTests : SharedMethods.SharedMethods
    {
        private readonly string _caseUrn;
        private readonly int _caseId;
        private readonly Tracker _tracker;

        private readonly Mock<IPipelineClient> _mockPipelineClient;
        private readonly Mock<IAuthorizationValidator> _mockTokenValidator;

        private readonly PolarisPipelineGetTracker _polarisPipelineGetTracker;

        public PolarisPipelineGetTrackerTests()
        {
            var fixture = new Fixture();
            _caseUrn = fixture.Create<string>();
            _caseId = fixture.Create<int>();
            _tracker = fixture.Create<Tracker>();
            fixture.Create<Guid>();

            var mockLogger = new Mock<ILogger<PolarisPipelineGetTracker>>();
            _mockPipelineClient = new Mock<IPipelineClient>();
            _mockTokenValidator = new Mock<IAuthorizationValidator>();

            _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new ValidateTokenResult { IsValid = true, UserName = "user-name" });
            _mockPipelineClient.Setup(client => client.GetTrackerAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Guid>()))
                .ReturnsAsync(_tracker);

            _polarisPipelineGetTracker = new PolarisPipelineGetTracker(mockLogger.Object, _mockPipelineClient.Object, _mockTokenValidator.Object);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenAccessCorrelationIdIsMissing()
        {
            var response = await _polarisPipelineGetTracker.Run(CreateHttpRequestWithoutCorrelationId(), _caseUrn, _caseId);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenAccessTokenIsMissing()
        {
            var response = await _polarisPipelineGetTracker.Run(CreateHttpRequestWithoutToken(), _caseUrn, _caseId);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsUnauthorizedWhenAccessTokenIsInvalid()
        {
            _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new ValidateTokenResult { IsValid = false });
            var response = await _polarisPipelineGetTracker.Run(CreateHttpRequest(), _caseUrn, _caseId);

            response.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenCmsAuthValuesIsMissing()
        {
            var response = await _polarisPipelineGetTracker.Run(CreateHttpRequestWithoutCmsAuthValuesToken(), _caseUrn, _caseId);

            response.Should().BeOfType<ObjectResult>();
            ((response as ObjectResult)?.StatusCode).Should().Be(403);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Run_ReturnsBadRequestWhenCaseUrnIsEmpty(string caseUrn)
        {
            var response = await _polarisPipelineGetTracker.Run(CreateHttpRequest(), caseUrn, _caseId);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsNotFoundWhenPipelineClientReturnsNull()
        {
            _mockPipelineClient.Setup(client => client.GetTrackerAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Guid>()))
                .ReturnsAsync(default(Tracker));

            var response = await _polarisPipelineGetTracker.Run(CreateHttpRequest(), _caseUrn, _caseId);

            response.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsOk()
        {
            var response = await _polarisPipelineGetTracker.Run(CreateHttpRequest(), _caseUrn, _caseId);

            response.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsTracker()
        {
            var response = await _polarisPipelineGetTracker.Run(CreateHttpRequest(), _caseUrn, _caseId) as OkObjectResult;

            response?.Value.Should().Be(_tracker);
        }

        [Fact]
        public async Task Run_ReturnsInternalServerErrorWhenHttpExceptionOccurs()
        {
            _mockPipelineClient.Setup(client => client.GetTrackerAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Guid>()))
                .ThrowsAsync(new HttpRequestException());

            var response = await _polarisPipelineGetTracker.Run(CreateHttpRequest(), _caseUrn, _caseId) as ObjectResult;

            response?.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task Run_ReturnsInternalServerErrorWhenUnhandledExceptionOccurs()
        {
            _mockPipelineClient.Setup(client => client.GetTrackerAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Guid>()))
                .ThrowsAsync(new Exception());

            var response = await _polarisPipelineGetTracker.Run(CreateHttpRequest(), _caseUrn, _caseId) as ObjectResult;

            response?.StatusCode.Should().Be(500);
        }
    }
}

