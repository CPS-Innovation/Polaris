// <copyright file="UpdateStatementTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions.HouseKeeping;

using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Interfaces.Model;
using Cps.Fct.Hk.Ui.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using Microsoft.Extensions.Logging;
using Cps.Fct.Hk.Ui.Services.Validators;
using System.Text;
using System.Text.Json;
using PolarisGateway.Functions.HouseKeeping;
using Common.Dto.Request.HouseKeeping;
using System;
using System.IO;
using Common.Dto.Response.HouseKeeping;
using Common.Dto.Request;
using Common.Constants;

/// <summary>
/// Unit tests for UpdateStatement.
/// </summary>
public class UpdateStatementTests
{
    private readonly TestLogger<UpdateStatement> mockLogger;
    private readonly Mock<ICommunicationService> mockCommunicationService;
    private readonly UpdateStatement sutUpdateStatement;
    private readonly UpdateStatementRequestValidator validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateStatementTests"/> class.
    /// </summary>
    public UpdateStatementTests()
    {
        this.mockLogger = new TestLogger<UpdateStatement>();
        this.mockCommunicationService = new Mock<ICommunicationService>();
        this.validator = new UpdateStatementRequestValidator();

        this.sutUpdateStatement = new UpdateStatement(
            this.mockLogger,
            this.mockCommunicationService.Object,
            this.validator);
    }

    /// <summary>
    ///  Tests that a valid request returns an Ok result with a success status.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ReturnsOkResult_WhenValidRequestProvided()
    {
        // Arrange and setup
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";

        var requestBody = new UpdateStatementRequest(Guid.NewGuid(), 43231, 3456, 6565, new DateOnly(2025, 09, 18), 5, true);
        string httpRequestString = JsonSerializer.Serialize(requestBody);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var expectedResponse = new UpdateStatementResponse(new UpdateStatementData
        {
            Id = 1234,
        });

        this.mockCommunicationService
            .Setup(x => x.UpdateStatementAsync(It.IsAny<int>(), It.IsAny<UpdateStatementRequest>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ReturnsAsync(expectedResponse);

        // Act
        IActionResult result = await this.sutUpdateStatement.Run(httpRequest, 4321, 123);

        // Assert
        OkObjectResult okObjectResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okObjectResult);
        Assert.Equal(1234, (okObjectResult.Value as UpdateStatementResponse)?.UpdateStatementData?.Id);

        Assert.Contains(this.mockLogger.Logs, log =>
          log.LogLevel == LogLevel.Information &&
          log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} UpdateStatement function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [4321] UpdateStatement function completed"));
    }

    /// <summary>
    ///  Tests that the function returns an unprocessable entity error when an invalid operation exception is thrown.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityError_WhenInvalidOperationExceptionIsThrown()
    {
        // Arrange and setup
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";

        var requestBody = new UpdateStatementRequest(Guid.NewGuid(), 43231, 3456, 6565, new DateOnly(2025, 09, 18), 5, true);
        string httpRequestString = JsonSerializer.Serialize(requestBody);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var expectedResponse = new UpdateStatementResponse(new UpdateStatementData
        {
            Id = 1234,
        });

        this.mockCommunicationService
            .Setup(x => x.UpdateStatementAsync(It.IsAny<int>(), It.IsAny<UpdateStatementRequest>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ThrowsAsync(new InvalidOperationException("Invalid operation error"));

        // Act
        IActionResult result = await this.sutUpdateStatement.Run(httpRequest, 4321, 123);

        // Assert
        ObjectResult objectResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} UpdateStatement function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} UpdateStatement function encountered an invalid operation error: Invalid operation error"));
    }

    /// <summary>
    ///  Tests that the function returns an unprocessable entity error when a not supported exception is thrown.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityError_WhenNotSupportedExceptionIsThrown()
    {
        // Arrange and setup
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";

        var requestBody = new UpdateStatementRequest(Guid.NewGuid(), 43231, 3456, 6565, new DateOnly(2025, 09, 18), 5, true);
        string httpRequestString = JsonSerializer.Serialize(requestBody);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var expectedResponse = new UpdateStatementResponse(new UpdateStatementData
        {
            Id = 1234,
        });

        this.mockCommunicationService
            .Setup(x => x.UpdateStatementAsync(It.IsAny<int>(), It.IsAny<UpdateStatementRequest>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ThrowsAsync(new NotSupportedException("Not supported error"));

        // Act
        IActionResult result = await this.sutUpdateStatement.Run(httpRequest, 4321, 123);

        // Assert
        ObjectResult objectResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} UpdateStatement function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} UpdateStatement function encountered an unsupported content type error: Not supported error"));
    }

    /// <summary>
    ///  Tests that the function returns an unauthorized error when unauthorized exception is thrown.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnauthorizedError_WhenUnauthorizedAccessExceptionIsThrown()
    {
        // Arrange and setup
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";

        var requestBody = new UpdateStatementRequest(Guid.NewGuid(), 43231, 3456, 6565, new DateOnly(2025, 09, 18), 5, true);
        string httpRequestString = JsonSerializer.Serialize(requestBody);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var expectedResponse = new UpdateStatementResponse(new UpdateStatementData
        {
            Id = 1234,
        });

        this.mockCommunicationService
            .Setup(x => x.UpdateStatementAsync(It.IsAny<int>(), It.IsAny<UpdateStatementRequest>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ThrowsAsync(new UnauthorizedAccessException("Unauthorized"));

        // Act
        IActionResult result = await this.sutUpdateStatement.Run(httpRequest, 4321, 123);

        // Assert
        UnauthorizedObjectResult objectResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, objectResult.StatusCode);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} UpdateStatement function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} UpdateStatement function encountered an unauthorized access error: Unauthorized"));
    }
}
