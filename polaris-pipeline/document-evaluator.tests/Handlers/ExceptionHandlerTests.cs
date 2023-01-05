using System.Net;
using AutoFixture;
using Azure;
using Common.Domain.Exceptions;
using Common.Exceptions.Contracts;
using document_evaluator.Domain.Handlers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace document_evaluation.tests.Handlers
{
    public class ExceptionHandlerTests
    {
        private readonly IExceptionHandler _exceptionHandler;
        private readonly Guid _correlationId;
        private readonly string _source;
        private readonly Mock<ILogger> _mockLogger;

        public ExceptionHandlerTests()
        {
            var fixture = new Fixture();

            _correlationId = fixture.Create<Guid>();
            _source = fixture.Create<string>();
            _mockLogger = new Mock<ILogger>();

            _exceptionHandler = new ExceptionHandler();
        }

        [Fact]
        public void HandleException_ReturnsUnauthorizedWhenUnauthorizedExceptionOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleException(new UnauthorizedException("Test unauthorized exception"), _correlationId, _source, _mockLogger.Object);

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public void HandleException_ReturnsBadRequestWhenBadRequestExceptionOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleException(new BadRequestException("Test bad request exception", "id"), _correlationId, _source, _mockLogger.Object);

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public void HandleException_ReturnsInternalServerErrorWhenRequestFailedExceptionWithBadRequestOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleException(
                new RequestFailedException((int)HttpStatusCode.BadRequest, "Test request failed exception"), _correlationId, _source, _mockLogger.Object);

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public void HandleException_ReturnsInternalServerErrorWhenRequestFailedExceptionWithNotFoundOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleException(
                new RequestFailedException((int)HttpStatusCode.NotFound, "Test request failed exception"), _correlationId, _source, _mockLogger.Object);

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public void HandleException_ReturnsExpectedStatusCodeWhenRequestFailedExceptionOccurs()
        {
            var expectedStatusCode = HttpStatusCode.ExpectationFailed;
            var httpResponseMessage = _exceptionHandler.HandleException(
                new RequestFailedException((int)expectedStatusCode, "Test request failed exception"), _correlationId, _source, _mockLogger.Object);

            httpResponseMessage.StatusCode.Should().Be(expectedStatusCode);
        }

        [Fact]
        public void HandleException_ReturnsInternalServerErrorWhenUnhandledErrorOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleException(new ApplicationException(), _correlationId, _source, _mockLogger.Object);

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }
    }
}
