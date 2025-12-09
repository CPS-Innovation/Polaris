// <copyright file="UpdateExhibitTests.cs" company="TheCrownProsecutionService">
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
/// Unit tests for UpdateExhibit.
/// </summary>
public class UpdateExhibitTests
{
    private readonly TestLogger<UpdateExhibit> mockLogger;
    private readonly Mock<ICommunicationService> mockCommunicationService;
    private readonly UpdateExhibit sutUpdateExhibit;
    private readonly UpdateExhibitRequestValidator validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateExhibitTests"/> class.
    /// </summary>
    public UpdateExhibitTests()
    {
        mockLogger = new TestLogger<UpdateExhibit>();
        mockCommunicationService = new Mock<ICommunicationService>();

        validator = new UpdateExhibitRequestValidator();

        sutUpdateExhibit = new UpdateExhibit(
            mockLogger,
            mockCommunicationService.Object,
            validator);
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

        var requestBody = new UpdateExhibitRequest(Guid.NewGuid(), 43231, 3456, "some-item", 6565, "some-ref", "some-subject", true, "some-new-prod", null);
        string httpRequestString = JsonSerializer.Serialize(requestBody);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var expectedResponse = new UpdateExhibitResponse(new UpdateExhibitData
        {
            Id = 1234,
        });

        mockCommunicationService
            .Setup(x => x.UpdateExhibitAsync(It.IsAny<int>(), It.IsAny<UpdateExhibitRequest>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ReturnsAsync(expectedResponse);

        // Act
        IActionResult result = await sutUpdateExhibit.Run(httpRequest, 4321, 1212);

        // Assert
        OkObjectResult okObjectResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okObjectResult);
        Assert.Equal(1234, (okObjectResult.Value as UpdateExhibitResponse)?.UpdateExhibitData?.Id);

        Assert.Contains(mockLogger.Logs, log =>
          log.LogLevel == LogLevel.Information &&
          log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} UpdateExhibit function processed a request."));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [4321] UpdateExhibit function completed"));
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

        var requestBody = new UpdateExhibitRequest(Guid.NewGuid(), 43231, 3456, "some-item", 6565, "some-ref", "some-subject", true, "some-new-prod", null);
        string httpRequestString = JsonSerializer.Serialize(requestBody);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var expectedResponse = new UpdateExhibitResponse(new UpdateExhibitData
        {
            Id = 1234,
        });

        mockCommunicationService
            .Setup(x => x.UpdateExhibitAsync(It.IsAny<int>(), It.IsAny<UpdateExhibitRequest>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ThrowsAsync(new InvalidOperationException("Invalid operation error"));

        // Act
        IActionResult result = await sutUpdateExhibit.Run(httpRequest, 4321, 1212);

        // Assert
        ObjectResult objectResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} UpdateExhibit function processed a request."));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} UpdateExhibit function encountered an invalid operation error: Invalid operation error"));
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

        var requestBody = new UpdateExhibitRequest(Guid.NewGuid(), 43231, 3456, "some-item", 6565, "some-ref", "some-subject", true, "some-new-prod", null);
        string httpRequestString = JsonSerializer.Serialize(requestBody);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var expectedResponse = new UpdateExhibitResponse(new UpdateExhibitData
        {
            Id = 1234,
        });

        mockCommunicationService
            .Setup(x => x.UpdateExhibitAsync(It.IsAny<int>(), It.IsAny<UpdateExhibitRequest>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ThrowsAsync(new NotSupportedException("Not supported error"));

        // Act
        IActionResult result = await sutUpdateExhibit.Run(httpRequest, 4321, 1212);

        // Assert
        ObjectResult objectResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} UpdateExhibit function processed a request."));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} UpdateExhibit function encountered an unsupported content type error: Not supported error"));
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

        var requestBody = new UpdateExhibitRequest(Guid.NewGuid(), 43231, 3456, "some-item", 6565, "some-ref", "some-subject", true, "some-new-prod", null);
        string httpRequestString = JsonSerializer.Serialize(requestBody);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var expectedResponse = new UpdateExhibitResponse(new UpdateExhibitData
        {
            Id = 1234,
        });

        mockCommunicationService
            .Setup(x => x.UpdateExhibitAsync(It.IsAny<int>(), It.IsAny<UpdateExhibitRequest>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ThrowsAsync(new UnauthorizedAccessException("Unauthorized"));

        // Act
        IActionResult result = await sutUpdateExhibit.Run(httpRequest, 4321, 1212);

        // Assert
        UnauthorizedObjectResult objectResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, objectResult.StatusCode);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} UpdateExhibit function processed a request."));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} UpdateExhibit function encountered an unauthorized access error: Unauthorized"));
    }
}
