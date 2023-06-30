using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Common.Validators.Contracts;
using FluentAssertions;
using Gateway.Clients.PolarisPipeline.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using PolarisGateway.Domain.PolarisPipeline;
using PolarisGateway.Domain.Validation;
using PolarisGateway.Factories.Contracts;
using PolarisGateway.Functions.PolarisPipeline.Case;
using Common.Telemetry.Wrappers.Contracts;
using Xunit;

namespace PolarisGateway.Tests.Functions.PolarisPipeline
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

        private readonly PolarisPipelineCase _polarisPipelineTriggerCoordinator;

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
                .ReturnsAsync(new StatusCodeResult((int)HttpStatusCode.Accepted));

            var mockTriggerCoordinatorResponseFactory = new Mock<ITriggerCoordinatorResponseFactory>();
            mockTriggerCoordinatorResponseFactory
                .Setup(factory => factory.Create(_request, It.IsAny<Guid>()))
                .Returns(_triggerCoordinatorResponse);

            _mockTokenValidator = new Mock<IAuthorizationValidator>();
            _mockTokenValidator
                .Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ValidateTokenResult { IsValid = true, UserName = "user-name" });

            _mockTelemetryAugmentationWrapper = new Mock<ITelemetryAugmentationWrapper>();
            _mockTelemetryAugmentationWrapper.Setup(wrapper => wrapper.AugmentRequestTelemetry(It.IsAny<string>(), It.IsAny<Guid>()));

            _polarisPipelineTriggerCoordinator =
                new PolarisPipelineCase(mockLogger.Object, _mockPipelineClient.Object, _mockTokenValidator.Object, mockTriggerCoordinatorResponseFactory.Object, _mockTelemetryAugmentationWrapper.Object);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenAccessCorrelationIdIsMissing()
        {
            var response = await _polarisPipelineTriggerCoordinator.Run(CreateHttpRequestWithoutCorrelationId(), _caseUrn, _caseId);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenAccessTokenIsMissing()
        {
            var response = await _polarisPipelineTriggerCoordinator.Run(CreateHttpRequestWithoutToken(), _caseUrn, _caseId);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenCmsAuthValuesIsMissing()
        {
            var response = await _polarisPipelineTriggerCoordinator.Run(CreateHttpRequestWithoutCmsAuthValuesToken(), _caseUrn, _caseId);

            response.Should().BeOfType<ObjectResult>();
            ((response as ObjectResult)?.StatusCode).Should().Be(403);
        }

        [Fact]
        public async Task Run_ReturnsUnauthorizedWhenAccessTokenIsInvalid()
        {
            _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new ValidateTokenResult { IsValid = false });
            var response = await _polarisPipelineTriggerCoordinator.Run(CreateHttpRequest(), _caseUrn, _caseId);

            response.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenCaseUrnIsNotSupplied()
        {
            var response = await _polarisPipelineTriggerCoordinator.Run(_request, string.Empty, _caseId);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Run_TriggersCoordinator()
        {
            _request.Method = "POST";
            await _polarisPipelineTriggerCoordinator.Run(_request, _caseUrn, _caseId);

            _mockPipelineClient.Verify(client => client.RefreshCaseAsync(_caseUrn, _caseId, It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [Fact]
        public async Task Run_NewRefreshProcessReturnsAccepted()
        {
            _request.Method = "POST";
            var result = await _polarisPipelineTriggerCoordinator.Run(_request, _caseUrn, _caseId) as ObjectResult;

            result.StatusCode.Should().Be((int)HttpStatusCode.Accepted);
        }

        [Fact]
        public async Task Run_ReturnsTriggerCoordinatorResponse()
        {
            var response = await _polarisPipelineTriggerCoordinator.Run(_request, _caseUrn, _caseId) as OkObjectResult;

            response?.Value.Should().Be(_triggerCoordinatorResponse);
        }

        [Fact]
        public async Task Run_ReturnsInternalServerErrorWhenHttpExceptionOccurs()
        {
            _mockPipelineClient.Setup(client => client.RefreshCaseAsync(_caseUrn, _caseId, It.IsAny<string>(), It.IsAny<Guid>()))
                .ThrowsAsync(new HttpRequestException());

            var response = await _polarisPipelineTriggerCoordinator.Run(_request, _caseUrn, _caseId) as ObjectResult;

            response?.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task Run_ReturnsInternalServerErrorWhenUnhandledExceptionOccurs()
        {
            _mockPipelineClient.Setup(client => client.RefreshCaseAsync(_caseUrn, _caseId, It.IsAny<string>(), It.IsAny<Guid>()))
                .ThrowsAsync(new Exception());

            var response = await _polarisPipelineTriggerCoordinator.Run(_request, _caseUrn, _caseId) as ObjectResult;

            response?.StatusCode.Should().Be(500);
        }
    }
}

