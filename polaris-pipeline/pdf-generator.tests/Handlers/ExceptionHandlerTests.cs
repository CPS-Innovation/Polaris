using System;
using System.Net;
using System.Net.Http;
using AutoFixture;
using Azure;
using Common.Domain.Exceptions;
using Common.Exceptions.Contracts;
using Ddei.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using pdf_generator.Domain.Exceptions;
using pdf_generator.Handlers;
using Xunit;

namespace pdf_generator.tests.Handlers
{
    public class ExceptionHandlerTests
    {
        private readonly IExceptionHandler _exceptionHandler;
        private readonly Guid _correlationId;
        private readonly string _source;
        private readonly Mock<ILogger> _loggerMock;

        public ExceptionHandlerTests()
        {
            var fixture = new Fixture();

            _correlationId = fixture.Create<Guid>();
            _source = fixture.Create<string>();
            _loggerMock = new Mock<ILogger>();

            _exceptionHandler = new ExceptionHandler();
        }

        [Fact]
        public void HandleException_ReturnsUnauthorizedWhenUnauthorizedExceptionOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleException(new UnauthorizedException("Test unauthorized exception"), _correlationId, _source, _loggerMock.Object);

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public void HandleException_ReturnsBadRequestWhenBadRequestExceptionOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleException(new BadRequestException("Test bad request exception", "id"), _correlationId, _source, _loggerMock.Object);

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public void HandleException_ReturnsBadRequestWhenFileTypeNotSupportedExceptionOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleException(new UnsupportedFileTypeException("Test file type"), _correlationId, _source, _loggerMock.Object);

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public void HandleException_ReturnsInternalServerErrorWhenHttpExceptionWithBadRequestOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleException(
                new DdeiClientException(HttpStatusCode.BadRequest, new HttpRequestException()), _correlationId, _source, _loggerMock.Object);

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public void HandleException_ReturnsExpectedStatusCodeWhenHttpExceptionOccurs()
        {
            var expectedStatusCode = HttpStatusCode.ExpectationFailed;
            var httpResponseMessage = _exceptionHandler.HandleException(
                new DdeiClientException(expectedStatusCode, new HttpRequestException()), _correlationId, _source, _loggerMock.Object);

            httpResponseMessage.StatusCode.Should().Be(expectedStatusCode);
        }

        [Fact]
        public void HandleException_ReturnsInternalServerErrorWhenRequestFailedExceptionWithBadRequestOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleException(
                new RequestFailedException((int)HttpStatusCode.BadRequest, "Test request failed exception"), _correlationId, _source, _loggerMock.Object);

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public void HandleException_ReturnsInternalServerErrorWhenRequestFailedExceptionWithNotFoundOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleException(
                new RequestFailedException((int)HttpStatusCode.NotFound, "Test request failed exception"), _correlationId, _source, _loggerMock.Object);

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public void HandleException_ReturnsExpectedStatusCodeWhenRequestFailedExceptionOccurs()
        {
            var expectedStatusCode = HttpStatusCode.ExpectationFailed;
            var httpResponseMessage = _exceptionHandler.HandleException(
                new RequestFailedException((int)expectedStatusCode, "Test request failed exception"), _correlationId, _source, _loggerMock.Object);

            httpResponseMessage.StatusCode.Should().Be(expectedStatusCode);
        }

        [Fact]
        public void HandleException_ReturnsNotImplementedWhenFailedToConvertToPdfExceptionOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleException(new PdfConversionException("Test id", "Test message"), _correlationId, _source, _loggerMock.Object);

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
        }

        [Fact]
        public void HandleException_ReturnsInternalServerErrorWhenUnhandledErrorOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleException(new ApplicationException(), _correlationId, _source, _loggerMock.Object);

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }
    }
}
