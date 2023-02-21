using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using PolarisGateway.Clients.PolarisPipeline;
using PolarisGateway.Domain.PolarisPipeline;
using PolarisGateway.Domain.Validators;
using PolarisGateway.Factories.Contracts;
using PolarisGateway.Functions.PolarisPipeline;
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

        private readonly PolarisPipelineTriggerCoordinator _polarisPipelineTriggerCoordinator;

        public PolarisPipelineTriggerCoordinatorTests()
        {
            var fixture = new Fixture();
            _caseUrn = fixture.Create<string>();
            _caseId = fixture.Create<int>();
            fixture.Create<string>();
            _request = CreateHttpRequest();
            _triggerCoordinatorResponse = fixture.Create<TriggerCoordinatorResponse>();

            var mockLogger = new Mock<ILogger<PolarisPipelineTriggerCoordinator>>();
            _mockPipelineClient = new Mock<IPipelineClient>();
            var mockTriggerCoordinatorResponseFactory = new Mock<ITriggerCoordinatorResponseFactory>();

            mockTriggerCoordinatorResponseFactory.Setup(factory => factory.Create(_request, It.IsAny<Guid>())).Returns(_triggerCoordinatorResponse);

            _mockTokenValidator = new Mock<IAuthorizationValidator>();

            _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            _polarisPipelineTriggerCoordinator =
                new PolarisPipelineTriggerCoordinator(mockLogger.Object, _mockPipelineClient.Object, mockTriggerCoordinatorResponseFactory.Object, _mockTokenValidator.Object);
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
            _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
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
        public async Task Run_ReturnsBadRequestWhenForceIsNotABool()
        {
            _request.Query = new QueryCollection(new Dictionary<string, StringValues> { { "force", new StringValues("not a bool") } });
            var response = await _polarisPipelineTriggerCoordinator.Run(_request, _caseUrn, _caseId);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Run_TriggersCoordinator()
        {
            await _polarisPipelineTriggerCoordinator.Run(_request, _caseUrn, _caseId);

            _mockPipelineClient.Verify(client => client.TriggerCoordinatorAsync(_caseUrn, _caseId, It.IsAny<string>(), false, It.IsAny<Guid>()));
        }

        [Fact]
        public async Task Run_ReturnsOk()
        {
            var response = await _polarisPipelineTriggerCoordinator.Run(_request, _caseUrn, _caseId);

            response.Should().BeOfType<OkObjectResult>();
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
            _mockPipelineClient.Setup(client => client.TriggerCoordinatorAsync(_caseUrn, _caseId, It.IsAny<string>(), false, It.IsAny<Guid>()))
                .ThrowsAsync(new HttpRequestException());

            var response = await _polarisPipelineTriggerCoordinator.Run(_request, _caseUrn, _caseId) as ObjectResult;

            response?.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task Run_ReturnsInternalServerErrorWhenUnhandledExceptionOccurs()
        {
            _mockPipelineClient.Setup(client => client.TriggerCoordinatorAsync(_caseUrn, _caseId, It.IsAny<string>(), false, It.IsAny<Guid>()))
                .ThrowsAsync(new Exception());

            var response = await _polarisPipelineTriggerCoordinator.Run(_request, _caseUrn, _caseId) as ObjectResult;

            response?.StatusCode.Should().Be(500);
        }
    }
}

