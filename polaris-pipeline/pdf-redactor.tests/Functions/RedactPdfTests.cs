using AutoFixture;
using Moq;
using pdf_redactor.Functions;
using System.Net;
using Common.Domain.Exceptions;
using FluentAssertions;
using Newtonsoft.Json;
using pdf_redactor.Services.DocumentRedaction;
using Xunit;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Common.Wrappers.Contracts;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Handlers.Contracts;
using Common.Telemetry.Wrappers.Contracts;

namespace pdf_redactor.tests.Functions
{
    public class RedactPdfTests
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IJsonConvertWrapper> _mockJsonConvertWrapper;
        private readonly Mock<IExceptionHandler> _mockExceptionHandler;
        private readonly Mock<ILogger<RedactPdf>> _loggerMock;
        private readonly Mock<IValidator<RedactPdfRequestDto>> _mockValidator;
        private readonly RedactPdf _redactPdf;
        private readonly string _caseUrn;
        private readonly string _caseId;
        private readonly string _documentId;
        private readonly string _serializedRedactPdfRequest;
        private readonly string _serializedRedactPdfResponse;

        public RedactPdfTests()
        {
            var request = _fixture.Create<RedactPdfRequestDto>();

            _serializedRedactPdfRequest = JsonConvert.SerializeObject(request);

            var redactPdfResponse = _fixture.Create<RedactPdfResponse>();
            _serializedRedactPdfResponse = JsonConvert.SerializeObject(redactPdfResponse);

            _mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
            _mockExceptionHandler = new Mock<IExceptionHandler>();
            var mockDocumentRedactionService = new Mock<IDocumentRedactionService>();

            _mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<RedactPdfRequestDto>(It.IsAny<string>())).Returns(request);
            _mockJsonConvertWrapper.Setup(wrapper => wrapper.SerializeObject(It.IsAny<RedactPdfResponse>()))
                .Returns(_serializedRedactPdfResponse);

            mockDocumentRedactionService.Setup(x => x.RedactPdfAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<RedactPdfRequestDto>(), It.IsAny<Guid>())).ReturnsAsync(redactPdfResponse);

            _loggerMock = new Mock<ILogger<RedactPdf>>();

            _mockValidator = new Mock<IValidator<RedactPdfRequestDto>>();
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<RedactPdfRequestDto>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _caseUrn = _fixture.Create<string>();
            _caseId = _fixture.Create<string>();
            _documentId = _fixture.Create<string>();

            var mockTelemetryAugmentationWrapper = new Mock<ITelemetryAugmentationWrapper>();
            _redactPdf = new RedactPdf(
                _mockExceptionHandler.Object,
                _mockJsonConvertWrapper.Object,
                mockDocumentRedactionService.Object,
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

            _mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<RedactPdfRequestDto>(It.IsAny<string>())).Returns(new RedactPdfRequestDto());

            var mockRequest = CreateMockRequest(new StringContent("{}"), null);

            var response = await _redactPdf.Run(mockRequest.Object, _caseUrn, _caseId, _documentId);

            response.Should().Be(errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenUsingAnInvalidCorrelationId()
        {
            var errorHttpResponseMessage = new ObjectResult("Error") { StatusCode = (int)HttpStatusCode.BadRequest };
            _mockExceptionHandler
                .Setup(handler => handler.HandleExceptionNew(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
                .Returns(errorHttpResponseMessage);

            var mockRequest = CreateMockRequest(_serializedRedactPdfRequest, null);

            var response = await _redactPdf.Run(mockRequest.Object, _caseUrn, _caseId, _documentId);

            response.Should().Be(errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenUsingAnEmptyCorrelationId()
        {
            var errorHttpResponseMessage = new ObjectResult("Error") { StatusCode = (int)HttpStatusCode.BadRequest };
            _mockExceptionHandler
                .Setup(handler => handler.HandleExceptionNew(It.IsAny<BadRequestException>(), It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
                .Returns(errorHttpResponseMessage);

            var mockRequest = CreateMockRequest(_serializedRedactPdfRequest, Guid.Empty);

            var response = await _redactPdf.Run(mockRequest.Object, _caseUrn, _caseId, _documentId);

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
            _mockValidator.Setup(v => v.ValidateAsync(It.IsAny<RedactPdfRequestDto>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(testFailures));

            var mockRequest = CreateMockRequest(_serializedRedactPdfRequest, Guid.NewGuid());

            var response = await _redactPdf.Run(mockRequest.Object, _caseUrn, _caseId, _documentId);

            response.Should().Be(errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsResponseWhenExceptionOccurs()
        {
            var errorHttpResponseMessage = new ObjectResult("Error") { StatusCode = (int)HttpStatusCode.InternalServerError };

            var exception = new Exception();
            _mockJsonConvertWrapper
                .Setup(wrapper => wrapper.SerializeObject(It.IsAny<RedactPdfResponse>()))
                .Throws(exception);
            _mockExceptionHandler
                .Setup(handler => handler.HandleExceptionNew(exception, It.IsAny<Guid>(), It.IsAny<string>(), _loggerMock.Object))
                .Returns(errorHttpResponseMessage);

            var mockRequest = CreateMockRequest(_serializedRedactPdfRequest, Guid.NewGuid());

            var response = await _redactPdf.Run(mockRequest.Object, _caseUrn, _caseId, _documentId);

            response.Should().Be(errorHttpResponseMessage);
        }

        [Fact]
        public async Task Run_ReturnsOk()
        {
            var mockRequest = CreateMockRequest(_serializedRedactPdfRequest, Guid.NewGuid());

            var response = await _redactPdf.Run(mockRequest.Object, _caseUrn, _caseId, _documentId);

            response.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Run_ReturnsExpectedContent()
        {
            var mockRequest = CreateMockRequest(_serializedRedactPdfRequest, Guid.NewGuid());

            var response = await _redactPdf.Run(mockRequest.Object, _caseUrn, _caseId, _documentId);

            response.Should().BeOfType<OkObjectResult>();

            var result = response as OkObjectResult;
            result.Should().NotBeNull();

            result?.Value?.Should().BeOfType<string>();
            var message = result?.Value as string;

            message.Should().Be(_serializedRedactPdfResponse);
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