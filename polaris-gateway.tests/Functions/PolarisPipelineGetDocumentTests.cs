using System;
using System.IO;
using System.Threading.Tasks;
using AutoFixture;
using PolarisGateway.Domain.Validators;
using Common.ValueObjects;
using FluentAssertions;
using PolarisGateway.Clients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using PolarisGateway.Domain.Validation;
using PolarisGateway.Functions;
using Common.Telemetry.Wrappers.Contracts;
using Xunit;

namespace PolarisGateway.Tests.Functions
{
    public class PolarisPipelineGetDocumentTests : SharedMethods.SharedMethods
    {
        private readonly string _blobName;
        private readonly string _caseUrn;
        private readonly int _caseId;
        private readonly PolarisDocumentId _polarisDocumentId;
        private readonly Stream _blobStream;

        private readonly Mock<IPipelineClient> _mockPipelineClient;
        private readonly Mock<IAuthorizationValidator> _mockTokenValidator;

        private readonly Mock<ITelemetryAugmentationWrapper> _mockTelemetryAugmentationWrapper;

        private readonly PolarisPipelineGetDocument _polarisPipelineGetDocument;

        public PolarisPipelineGetDocumentTests()
        {
            var fixture = new Fixture();
            _caseUrn = fixture.Create<string>();
            _caseId = fixture.Create<int>();
            _polarisDocumentId = fixture.Create<PolarisDocumentId>();

            _blobName = fixture.Create<string>();
            _blobStream = new MemoryStream();

            var mockLogger = new Mock<ILogger<PolarisPipelineGetDocument>>();

            _mockTokenValidator = new Mock<IAuthorizationValidator>();
            _mockTokenValidator
                .Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new ValidateTokenResult { IsValid = true, UserName = "user-name" });

            _mockPipelineClient = new Mock<IPipelineClient>();
            _mockPipelineClient
                .Setup(client => client.GetDocumentAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<PolarisDocumentId>(), It.IsAny<Guid>()))
                .ReturnsAsync(_blobStream);

            _mockTelemetryAugmentationWrapper = new Mock<ITelemetryAugmentationWrapper>();
            _mockTelemetryAugmentationWrapper.Setup(wrapper => wrapper.RegisterUserName(It.IsAny<string>()));
            _mockTelemetryAugmentationWrapper.Setup(wrapper => wrapper.RegisterCorrelationId(It.IsAny<Guid>()));

            _polarisPipelineGetDocument = new PolarisPipelineGetDocument(_mockPipelineClient.Object, mockLogger.Object, _mockTokenValidator.Object, _mockTelemetryAugmentationWrapper.Object);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenAccessCorrelationIdIsMissing()
        {
            var response = await _polarisPipelineGetDocument.Run(CreateHttpRequestWithoutCorrelationId(), _caseUrn, _caseId, _polarisDocumentId.Value);

            response.Should().BeOfType<ObjectResult>()
                .And.Subject.As<ObjectResult>().StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenAccessTokenIsMissing()
        {
            var response = await _polarisPipelineGetDocument.Run(CreateHttpRequestWithoutToken(), _caseUrn, _caseId, _polarisDocumentId.Value);

            response.Should().BeOfType<ObjectResult>()
                .And.Subject.As<ObjectResult>().StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Run_ReturnsUnauthorizedWhenAccessTokenIsInvalid()
        {
            _mockTokenValidator.Setup(x => x.ValidateTokenAsync(It.IsAny<StringValues>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new ValidateTokenResult { IsValid = false });
            var response = await _polarisPipelineGetDocument.Run(CreateHttpRequest(), _caseUrn, _caseId, _polarisDocumentId.Value);
            response.Should().BeOfType<ObjectResult>()
                .And.Subject.As<ObjectResult>().StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task Run_ReturnsOk()
        {
            var response = await _polarisPipelineGetDocument.Run(CreateHttpRequest(), _caseUrn, _caseId, _polarisDocumentId.Value);

            response.Should().BeOfType<FileStreamResult>();
        }

        [Fact]
        public async Task Run_ReturnsBlobStream()
        {
            var response = await _polarisPipelineGetDocument.Run(CreateHttpRequest(), _caseUrn, _caseId, _polarisDocumentId.Value) as FileStreamResult;

            response.FileStream.Should().BeSameAs(_blobStream);
        }

        [Fact]
        public async Task Run_ReturnsInternalServerErrorWhenUnhandledExceptionOccurs()
        {
            _mockPipelineClient
                .Setup(client => client.GetDocumentAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<PolarisDocumentId>(), It.IsAny<Guid>()))
                .ThrowsAsync(new Exception());

            var response = await _polarisPipelineGetDocument.Run(CreateHttpRequest(), _caseUrn, _caseId, _polarisDocumentId.Value) as ObjectResult;

            response?.StatusCode.Should().Be(500);
        }
    }
}

