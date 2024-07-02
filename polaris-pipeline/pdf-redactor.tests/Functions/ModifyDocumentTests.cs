using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoFixture;
using Common.Dto.Request;
using Common.Exceptions;
using Common.Handlers;
using Common.Telemetry;
using Common.Wrappers;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Newtonsoft.Json;
using pdf_redactor.Functions;
using pdf_redactor.Services.DocumentManipulation;
using Xunit;

namespace pdf_redactor.tests.Functions
{
    public class ModifyDocumentTests
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
        private readonly Mock<IExceptionHandler> _mockExceptionHandler;
        private readonly Mock<ILogger<ModifyDocument>> _loggerMock;
        private readonly Mock<IValidator<ModifyDocumentWithDocumentDto>> _mockValidator;
        private readonly ModifyDocument _documentModifier;
        private readonly string _caseUrn;
        private readonly string _caseId;
        private readonly string _documentId;
        private readonly string _serializedModifyPdfRequest;

        public ModifyDocumentTests()
        {
            var request = _fixture.Create<ModifyDocumentWithDocumentDto>();

            _serializedModifyPdfRequest = JsonConvert.SerializeObject(request);

            _mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
            _mockExceptionHandler = new Mock<IExceptionHandler>();
            var mockDocumentManipulationService = new Mock<IDocumentManipulationService>();

            _mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<ModifyDocumentWithDocumentDto>(It.IsAny<string>())).Returns(request);

            mockDocumentManipulationService.Setup(x => x.RemoveOrRotatePagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ModifyDocumentWithDocumentDto>(), It.IsAny<Guid>())).ReturnsAsync(new MemoryStream());

            _loggerMock = new Mock<ILogger<ModifyDocument>>();

            _mockValidator = new Mock<IValidator<ModifyDocumentWithDocumentDto>>();
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ModifyDocumentWithDocumentDto>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _caseUrn = _fixture.Create<string>();
            _caseId = _fixture.Create<string>();
            _documentId = _fixture.Create<string>();

            var mockTelemetryAugmentationWrapper = new Mock<ITelemetryAugmentationWrapper>();
            _documentModifier = new ModifyDocument(
                _mockExceptionHandler.Object,
                _mockJsonConvertWrapper.Object,
                mockDocumentManipulationService.Object,
                _loggerMock.Object,
                _mockValidator.Object,
                mockTelemetryAugmentationWrapper.Object);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenContentIsInvalid()
        {
            var errorHttpResponseMessage = new ObjectResult("Error") { StatusCode = (int)HttpStatusCode.BadRequest };
            _mockExceptionHandler
                .Setup(handler => handler.HandleExceptionNew(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
                .Returns(errorHttpResponseMessage);

            _mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<RedactPdfRequestWithDocumentDto>(It.IsAny<string>())).Returns(new RedactPdfRequestWithDocumentDto());

            var mockRequest = CreateMockRequest(new StringContent("{}"), null);

            var response = await _documentModifier.Run(mockRequest.Object, _caseUrn, _caseId, _documentId);

            response.Should().Be(errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenUsingAnInvalidCorrelationId()
        {
            var errorHttpResponseMessage = new ObjectResult("Error") { StatusCode = (int)HttpStatusCode.BadRequest };
            _mockExceptionHandler
                .Setup(handler => handler.HandleExceptionNew(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
                .Returns(errorHttpResponseMessage);

            var mockRequest = CreateMockRequest(_serializedModifyPdfRequest, null);

            var response = await _documentModifier.Run(mockRequest.Object, _caseUrn, _caseId, _documentId);

            response.Should().Be(errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenUsingAnEmptyCorrelationId()
        {
            var errorHttpResponseMessage = new ObjectResult("Error") { StatusCode = (int)HttpStatusCode.BadRequest };
            _mockExceptionHandler
                .Setup(handler => handler.HandleExceptionNew(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
                .Returns(errorHttpResponseMessage);

            var mockRequest = CreateMockRequest(_serializedModifyPdfRequest, Guid.Empty);

            var response = await _documentModifier.Run(mockRequest.Object, _caseUrn, _caseId, _documentId);

            response.Should().Be(errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenThereAreAnyValidationErrors()
        {
            var errorHttpResponseMessage = new ObjectResult("Error") { StatusCode = (int)HttpStatusCode.BadRequest };

            var testFailures = _fixture.CreateMany<ValidationFailure>(2);

            _mockExceptionHandler
                .Setup(handler => handler.HandleExceptionNew(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
                .Returns(errorHttpResponseMessage);
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ModifyDocumentWithDocumentDto>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(testFailures));

            var mockRequest = CreateMockRequest(_serializedModifyPdfRequest, Guid.NewGuid());

            var response = await _documentModifier.Run(mockRequest.Object, _caseUrn, _caseId, _documentId);

            response.Should().Be(errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsOk()
        {
            var mockRequest = CreateMockRequest(_serializedModifyPdfRequest, Guid.NewGuid());

            var response = await _documentModifier.Run(mockRequest.Object, _caseUrn, _caseId, _documentId);

            response.Should().BeOfType<FileStreamResult>();
        }

        [Fact]
        public async Task Run_ReturnsExpectedContent()
        {
            var mockRequest = CreateMockRequest(_serializedModifyPdfRequest, Guid.NewGuid());

            var response = await _documentModifier.Run(mockRequest.Object, _caseUrn, _caseId, _documentId);

            response.Should().BeOfType<FileStreamResult>();

            var result = response as FileStreamResult;
            result.Should().NotBeNull();

            result?.Should().BeOfType<FileStreamResult>();
        }

        private static Mock<HttpRequest> CreateMockRequest(object body, Guid? correlationId)
        {
            var mockRequest = new Mock<HttpRequest>();

            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);

            var json = JsonConvert.SerializeObject(body);

            sw.Write(json);
            sw.Flush();

            ms.Position = 0;

            mockRequest.Setup(x => x.Body).Returns(ms);
            mockRequest.Setup(x => x.ContentLength).Returns(ms.Length);

            var mockHeaders = new HeaderDictionary();

            if (correlationId.HasValue && correlationId.Value != Guid.Empty)
            {
                mockHeaders.Append("Correlation-Id", correlationId.Value.ToString());
            }

            mockRequest.Setup(x => x.Headers).Returns(mockHeaders);
            return mockRequest;
        }
    }
}