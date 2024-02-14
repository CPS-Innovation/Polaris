using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using PolarisGateway.Domain.Validators;
using FluentAssertions;
using Gateway.Clients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using PolarisGateway.Domain.PolarisPipeline;
using PolarisGateway.Domain.Validation;
using PolarisGateway.Factories;
using PolarisGateway.Functions;
using Common.Telemetry.Wrappers.Contracts;
using Xunit;

namespace PolarisGateway.Tests.Functions
{
    public class PolarisPipelineTriggerCoordinatorTests : SharedMethods.SharedMethods
    {
        private readonly HttpRequest _request;
        private readonly string _caseUrn;
        private readonly int _caseId;
        private readonly TriggerCoordinatorResponse _triggerCoordinatorResponse;

        private readonly Mock<IPipelineClient> _mockPipelineClient;
        private readonly Mock<IAuthorizationValidator> _mockTokenValidator;

        private readonly Mock<ITelemetryAugmentationWrapper> _mockTelemetryAugmentationWrapper;

        private readonly PolarisPipelineCase _polarisPipelineCase;

        public PolarisPipelineTriggerCoordinatorTests()
        {
            var fixture = new Fixture();
            _caseUrn = fixture.Create<string>();
            _caseId = fixture.Create<int>();
            fixture.Create<string>();
            _request = CreateHttpRequest();
            _triggerCoordinatorResponse = fixture.Create<TriggerCoordinatorResponse>();

            var mockLogger = new Mock<ILogger<PolarisPipelineCase>>();

            _mockPipelineClient = new Mock<IPipelineClient>();
            _mockPipelineClient
                .Setup(x => x.RefreshCaseAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Guid>()))
                .ReturnsAsync(HttpStatusCode.Accepted);

            var mockTriggerCoordinatorResponseFactory = new Mock<ITrackerResponseFactory>();
            mockTriggerCoordinatorResponseFactory
                .Setup(factory => factory.Create(_request, It.IsAny<Guid>()))
                .Returns(_triggerCoordinatorResponse);

            _mockTokenValidator = new Mock<IAuthorizationValidator>();
            _mockTokenValidator
                .Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ValidateTokenResult { IsValid = true, UserName = "user-name" });

            _mockTelemetryAugmentationWrapper = new Mock<ITelemetryAugmentationWrapper>();
            _mockTelemetryAugmentationWrapper.Setup(wrapper => wrapper.RegisterUserName(It.IsAny<string>()));
            _mockTelemetryAugmentationWrapper.Setup(wrapper => wrapper.RegisterCorrelationId(It.IsAny<Guid>()));

            _polarisPipelineCase =
                new PolarisPipelineCase(mockLogger.Object, _mockPipelineClient.Object, _mockTokenValidator.Object, mockTriggerCoordinatorResponseFactory.Object, _mockTelemetryAugmentationWrapper.Object);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenAccessCorrelationIdIsMissing()
        {
            var response = await _polarisPipelineCase.Run(CreateHttpRequestWithoutCorrelationId(), _caseUrn, _caseId);

            response.Should().BeOfType<ObjectResult>()
                .And.Subject.As<ObjectResult>().StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenAccessTokenIsMissing()
        {
            var response = await _polarisPipelineCase.Run(CreateHttpRequestWithoutToken(), _caseUrn, _caseId);

            response.Should().BeOfType<ObjectResult>()
                .And.Subject.As<ObjectResult>().StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenCmsAuthValuesIsMissing()
        {
            var response = await _polarisPipelineCase.Run(CreateHttpRequestWithoutCmsAuthValuesToken(), _caseUrn, _caseId);

            response.Should().BeOfType<ObjectResult>()
                .And.Subject.As<ObjectResult>().StatusCode.Should().Be(403);
        }

        [Fact]
        public async Task Run_ReturnsUnauthorizedWhenAccessTokenIsInvalid()
        {
            _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new ValidateTokenResult { IsValid = false });
            var response = await _polarisPipelineCase.Run(CreateHttpRequest(), _caseUrn, _caseId);

            response.Should().BeOfType<ObjectResult>()
                .And.Subject.As<ObjectResult>().StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Run_TriggersCoordinator()
        {
            _request.Method = "POST";
            await _polarisPipelineCase.Run(_request, _caseUrn, _caseId);

            _mockPipelineClient.Verify(client => client.RefreshCaseAsync(_caseUrn, _caseId, It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public async Task Run_NewRefreshProcessReturnsAccepted()
        {
            _request.Method = "POST";
            var result = await _polarisPipelineCase.Run(_request, _caseUrn, _caseId) as ObjectResult;

            result.StatusCode.Should().Be((int)HttpStatusCode.Accepted);
        }

        [Fact]
        public async Task Run_ReturnsTriggerCoordinatorResponse()
        {
            var response = await _polarisPipelineCase.Run(_request, _caseUrn, _caseId) as OkObjectResult;

            response?.Value.Should().Be(_triggerCoordinatorResponse);
        }

        [Fact]
        public async Task Run_ReturnsInternalServerErrorWhenHttpExceptionOccurs()
        {
            _mockPipelineClient.Setup(client => client.RefreshCaseAsync(_caseUrn, _caseId, It.IsAny<string>(), It.IsAny<Guid>()))
                .ThrowsAsync(new HttpRequestException());

            var response = await _polarisPipelineCase.Run(_request, _caseUrn, _caseId) as ObjectResult;

            response?.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task Run_ReturnsInternalServerErrorWhenUnhandledExceptionOccurs()
        {
            _mockPipelineClient.Setup(client => client.RefreshCaseAsync(_caseUrn, _caseId, It.IsAny<string>(), It.IsAny<Guid>()))
                .ThrowsAsync(new Exception());

            var response = await _polarisPipelineCase.Run(_request, _caseUrn, _caseId) as ObjectResult;

            response?.StatusCode.Should().Be(500);
        }
    }
}

