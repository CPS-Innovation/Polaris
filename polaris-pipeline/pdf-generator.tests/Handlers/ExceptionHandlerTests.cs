using System;
using System.Net;
using System.Net.Http;
using AutoFixture;
using Azure;
using Common.Domain.Exceptions;
using Common.Exceptions;
using Common.Handlers;
using Common.Handlers.Contracts;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
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
            var httpResponseMessage = _exceptionHandler.HandleExceptionNew(new UnauthorizedException("Test unauthorized exception"), _correlationId, _source, _loggerMock.Object);

            httpResponseMessage.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        }

        [Fact]
        public void HandleException_ReturnsBadRequestWhenBadRequestExceptionOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleExceptionNew(new BadRequestException("Test bad request exception", "id"), _correlationId, _source, _loggerMock.Object);

            httpResponseMessage.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public void HandleException_ReturnsBadRequestWhenFileTypeNotSupportedExceptionOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleExceptionNew(new UnsupportedFileTypeException("Test file type"), _correlationId, _source, _loggerMock.Object);

            httpResponseMessage.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public void HandleException_ReturnsInternalServerErrorWhenHttpExceptionWithBadRequestOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleExceptionNew(
                new DdeiClientException(HttpStatusCode.BadRequest, new HttpRequestException()), _correlationId, _source, _loggerMock.Object);

            httpResponseMessage.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        [Fact]
        public void HandleException_ReturnsExpectedStatusCodeWhenHttpExceptionOccurs()
        {
            const HttpStatusCode expectedStatusCode = HttpStatusCode.ExpectationFailed;
            var httpResponseMessage = _exceptionHandler.HandleExceptionNew(
                new DdeiClientException(expectedStatusCode, new HttpRequestException()), _correlationId, _source, _loggerMock.Object);

            httpResponseMessage.StatusCode.Should().Be((int)expectedStatusCode);
        }

        [Fact]
        public void HandleException_ReturnsInternalServerErrorWhenRequestFailedExceptionWithBadRequestOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleExceptionNew(
                new RequestFailedException((int)HttpStatusCode.BadRequest, "Test request failed exception"), _correlationId, _source, _loggerMock.Object);

            httpResponseMessage.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        [Fact]
        public void HandleException_ReturnsInternalServerErrorWhenRequestFailedExceptionWithNotFoundOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleExceptionNew(
                new RequestFailedException((int)HttpStatusCode.NotFound, "Test request failed exception"), _correlationId, _source, _loggerMock.Object);

            httpResponseMessage.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        [Fact]
        public void HandleException_ReturnsExpectedStatusCodeWhenRequestFailedExceptionOccurs()
        {
            const HttpStatusCode expectedStatusCode = HttpStatusCode.ExpectationFailed;
            var httpResponseMessage = _exceptionHandler.HandleExceptionNew(
                new RequestFailedException((int)expectedStatusCode, "Test request failed exception"), _correlationId, _source, _loggerMock.Object);

            httpResponseMessage.StatusCode.Should().Be((int)expectedStatusCode);
        }

        [Fact]
        public void HandleException_ReturnsNotImplementedWhenFailedToConvertToPdfExceptionOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleExceptionNew(new PdfConversionException("Test id", "Test message"), _correlationId, _source, _loggerMock.Object);

            httpResponseMessage.StatusCode.Should().Be((int)HttpStatusCode.NotImplemented);
        }

        [Fact]
        public void HandleException_ReturnsInternalServerErrorWhenUnhandledErrorOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleExceptionNew(new ApplicationException(), _correlationId, _source, _loggerMock.Object);

            httpResponseMessage.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }
    }
}
