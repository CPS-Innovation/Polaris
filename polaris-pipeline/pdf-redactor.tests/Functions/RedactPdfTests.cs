using AutoFixture;
using Moq;
using pdf_redactor.Functions;
using System.Net;
using Common.Exceptions;
using FluentAssertions;
using pdf_redactor.Services.DocumentRedaction;
using Xunit;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Common.Wrappers;
using Common.Dto.Request;
using Common.Handlers;
using System.Text.Json;

namespace pdf_redactor.tests.Functions
{
    public class RedactPdfTests
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
        private readonly Mock<IExceptionHandler> _mockExceptionHandler;
        private readonly Mock<ILogger<RedactPdf>> _loggerMock;
        private readonly Mock<IValidator<RedactPdfRequestWithDocumentDto>> _mockValidator;
        private readonly RedactPdf _pdfRedactor;
        private readonly string _caseUrn;
        private readonly int _caseId;
        private readonly string _documentId;
        private readonly long _versionId;
        private readonly string _serializedRedactPdfRequest;

        public RedactPdfTests()
        {
            var request = _fixture.Create<RedactPdfRequestWithDocumentDto>();

            _serializedRedactPdfRequest = JsonSerializer.Serialize(request);

            _mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
            _mockExceptionHandler = new Mock<IExceptionHandler>();
            var mockDocumentRedactionService = new Mock<IDocumentRedactionService>();

            _mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<RedactPdfRequestWithDocumentDto>(It.IsAny<string>())).Returns(request);

            mockDocumentRedactionService.Setup(x => x.RedactAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<RedactPdfRequestWithDocumentDto>(), It.IsAny<Guid>())).ReturnsAsync(new MemoryStream());

            _loggerMock = new Mock<ILogger<RedactPdf>>();

            _mockValidator = new Mock<IValidator<RedactPdfRequestWithDocumentDto>>();
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<RedactPdfRequestWithDocumentDto>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _caseUrn = _fixture.Create<string>();
            _caseId = _fixture.Create<int>();
            _documentId = _fixture.Create<string>();
            _versionId = _fixture.Create<long>();

            _pdfRedactor = new RedactPdf(
                _mockExceptionHandler.Object,
                _mockJsonConvertWrapper.Object,
                mockDocumentRedactionService.Object,
                _loggerMock.Object,
                _mockValidator.Object);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenContentIsInvalid()
        {
            var errorHttpResponseMessage = new JsonResult("Error") { StatusCode = (int)HttpStatusCode.BadRequest };
            _mockExceptionHandler
                .Setup(handler => handler.HandleExceptionNew(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
                .Returns(errorHttpResponseMessage);

            _mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<RedactPdfRequestWithDocumentDto>(It.IsAny<string>())).Returns(new RedactPdfRequestWithDocumentDto());

            var mockRequest = CreateMockRequest(new StringContent("{}"), null);

            var response = await _pdfRedactor.Run(mockRequest.Object, _caseUrn, _caseId, _documentId, _versionId);

            response.Should().Be(errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenUsingAnInvalidCorrelationId()
        {
            var errorHttpResponseMessage = new JsonResult("Error") { StatusCode = (int)HttpStatusCode.BadRequest };
            _mockExceptionHandler
                .Setup(handler => handler.HandleExceptionNew(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
                .Returns(errorHttpResponseMessage);

            var mockRequest = CreateMockRequest(_serializedRedactPdfRequest, null);

            var response = await _pdfRedactor.Run(mockRequest.Object, _caseUrn, _caseId, _documentId, _versionId);

            response.Should().Be(errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenUsingAnEmptyCorrelationId()
        {
            var errorHttpResponseMessage = new JsonResult("Error") { StatusCode = (int)HttpStatusCode.BadRequest };
            _mockExceptionHandler
                .Setup(handler => handler.HandleExceptionNew(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
                .Returns(errorHttpResponseMessage);

            var mockRequest = CreateMockRequest(_serializedRedactPdfRequest, Guid.Empty);

            var response = await _pdfRedactor.Run(mockRequest.Object, _caseUrn, _caseId, _documentId, _versionId);

            response.Should().Be(errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenThereAreAnyValidationErrors()
        {
            var errorHttpResponseMessage = new JsonResult("Error") { StatusCode = (int)HttpStatusCode.BadRequest };

            var testFailures = _fixture.CreateMany<ValidationFailure>(2);

            _mockExceptionHandler
                .Setup(handler => handler.HandleExceptionNew(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
                .Returns(errorHttpResponseMessage);
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<RedactPdfRequestWithDocumentDto>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(testFailures));

            var mockRequest = CreateMockRequest(_serializedRedactPdfRequest, Guid.NewGuid());

            var response = await _pdfRedactor.Run(mockRequest.Object, _caseUrn, _caseId, _documentId, _versionId);

            response.Should().Be(errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsOk()
        {
            var mockRequest = CreateMockRequest(_serializedRedactPdfRequest, Guid.NewGuid());

            var response = await _pdfRedactor.Run(mockRequest.Object, _caseUrn, _caseId, _documentId, _versionId);

            response.Should().BeOfType<FileStreamResult>();
        }

        [Fact]
        public async Task Run_ReturnsExpectedContent()
        {
            var mockRequest = CreateMockRequest(_serializedRedactPdfRequest, Guid.NewGuid());

            var response = await _pdfRedactor.Run(mockRequest.Object, _caseUrn, _caseId, _documentId, _versionId);

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

            var json = JsonSerializer.Serialize(body);

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