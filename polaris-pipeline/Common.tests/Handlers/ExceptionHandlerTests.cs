using System.Net;
using AutoFixture;
using Azure;
using Common.Exceptions;
using Common.Handlers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Common.Tests.Handlers
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
        public void HandleException_ReturnsBadRequestWhenBadRequestExceptionOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleExceptionNew(new BadRequestException("Test bad request exception", "id"), _correlationId, _source, _loggerMock.Object);

            httpResponseMessage.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
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
        public void HandleException_ReturnsInternalServerErrorWhenUnhandledErrorOccurs()
        {
            var httpResponseMessage = _exceptionHandler.HandleExceptionNew(new ApplicationException(), _correlationId, _source, _loggerMock.Object);

            httpResponseMessage.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }


    }
}
