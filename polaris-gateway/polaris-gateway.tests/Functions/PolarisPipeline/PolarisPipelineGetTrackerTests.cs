using System;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Common.Dto.Tracker;
using Common.Validators.Contracts;
using FluentAssertions;
using Gateway.Clients.PolarisPipeline.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using PolarisGateway.Domain.Validation;
using PolarisGateway.Functions.PolarisPipeline.Case;
using PolarisGateway.Wrappers;
using Xunit;

namespace PolarisGateway.Tests.Functions.PolarisPipeline
{
    public class PolarisPipelineGetTrackerTests : SharedMethods.SharedMethods
    {
        private readonly string _caseUrn;
        private readonly int _caseId;
        private readonly TrackerDto _tracker;

        private readonly Mock<IPipelineClient> _mockPipelineClient;
        private readonly Mock<IAuthorizationValidator> _mockTokenValidator;

        private readonly Mock<ITelemetryAugmentationWrapper> _mockTelemetryAugmentationWrapper;

        private readonly PolarisPipelineGetCaseTracker _polarisPipelineGetTracker;

        public PolarisPipelineGetTrackerTests()
        {
            var fixture = new Fixture();
            _caseUrn = fixture.Create<string>();
            _caseId = fixture.Create<int>();
            _tracker = fixture.Create<TrackerDto>();
            fixture.Create<Guid>();

            var mockLogger = new Mock<ILogger<PolarisPipelineGetCaseTracker>>();
            _mockPipelineClient = new Mock<IPipelineClient>();
            _mockTokenValidator = new Mock<IAuthorizationValidator>();

            _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new ValidateTokenResult { IsValid = true, UserName = "user-name" });
            _mockPipelineClient.Setup(client => client.GetTrackerAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Guid>()))
                .ReturnsAsync(_tracker);

            _mockTelemetryAugmentationWrapper = new Mock<ITelemetryAugmentationWrapper>();
            _mockTelemetryAugmentationWrapper.Setup(wrapper => wrapper.AugmentRequestTelemetry(It.IsAny<string>(), It.IsAny<Guid>()));

            _polarisPipelineGetTracker = new PolarisPipelineGetCaseTracker(mockLogger.Object, _mockPipelineClient.Object, _mockTokenValidator.Object, _mockTelemetryAugmentationWrapper.Object);
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
                .ReturnsAsync(default(TrackerDto));

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

