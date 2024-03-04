using System;
using AutoFixture;
using Common.Domain.Exceptions;
using coordinator.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Helpers;

public class UnhandledExceptionHelperTests
{
    private readonly Fixture _fixture;

    private readonly Mock<ILogger> _logger;
    private readonly string _logName;
    private readonly Guid _correlationId;

    public UnhandledExceptionHelperTests()
    {
        _fixture = new Fixture();
        _logger = new Mock<ILogger>();
        _logName = _fixture.Create<string>();
        _correlationId = _fixture.Create<Guid>();

    }

    [Fact]
    public void HandleUnhandledException_WhenBadRequestException_ReturnsBadRequest()
    {
        // Arrange
        var exception = new BadRequestException(
            _fixture.Create<string>(),
            _fixture.Create<string>()
        );

        // Act
        var result = UnhandledExceptionHelper.HandleUnhandledException(_logger.Object, _logName, _correlationId, exception);

        // Assert
        result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void HandleUnhandledException_WhenUnhandledException_ReturnsInternalServerError()
    {
        // Arrange
        var exception = new Exception(_fixture.Create<string>());

        // Act
        var result = UnhandledExceptionHelper.HandleUnhandledException(_logger.Object, _logName, _correlationId, exception);

        // Assert
        result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}
