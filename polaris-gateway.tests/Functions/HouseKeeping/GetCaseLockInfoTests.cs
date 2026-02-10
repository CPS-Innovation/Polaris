// <copyright file="GetCaseLockInfoTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions.HouseKeeping;

using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Cps.Fct.Hk.Ui.Interfaces;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using Microsoft.Extensions.Logging;
using Common.Constants;
using Common.Dto.Response.HouseKeeping;
using Common.Dto.Request;
using System;
using PolarisGateway.Functions.HouseKeeping;

/// <summary>
/// Unit tests for the <see cref="GetCaseLockInfo"/> class.
/// </summary>
public class GetCaseLockInfoTests
{
    private readonly TestLogger<GetCaseLockInfo> mockLogger;
    private readonly Mock<ICaseLockService> caseLockService;
    private readonly GetCaseLockInfo getCaseLockInfoFunction;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCaseLockInfoTests"/> class.
    /// </summary>
    public GetCaseLockInfoTests()
    {
        // Initialize mocks
        this.mockLogger = new TestLogger<GetCaseLockInfo>();
        this.caseLockService = new Mock<ICaseLockService>();

        // Initialize the function class
        this.getCaseLockInfoFunction = new GetCaseLockInfo(this.mockLogger, this.caseLockService.Object);
    }

    /// <summary>
    /// Tests that the function returns an OK result when a valid request is provided.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsOkResult_WhenValidRequestProvided()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        // Set up a DefaultHttpContext to support setting headers
        var context = new DefaultHttpContext();
        mockRequest.Setup(r => r.HttpContext).Returns(context);

        var mockCaseInfo = new CaseLockedStatusResult
        {
            IsLocked = true,
        };

        // Ensure the mock returns the expected list of communications
        this.caseLockService
            .Setup(x => x.CheckCaseLockAsync(123, It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(mockCaseInfo);

        // Act
        IActionResult result = await this.getCaseLockInfoFunction.Run(mockRequest.Object, 123);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);

        // Check that the communications are not null
        Assert.NotNull(okResult.Value);

        // Ensure that the correct communications were returned
        CaseLockedStatusResult caseInfo = Assert.IsType<CaseLockedStatusResult>(okResult.Value);
        Assert.Equal(mockCaseInfo, caseInfo);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseLockInfo function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [123] GetCaseLockInfo function completed"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"caseId [123]"));
    }

    /// <summary>
    /// Tests that the function returns an unprocessable entity error when an invalid operation exception is thrown.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityError_WhenInvalidOperationExceptionIsThrown()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        this.caseLockService
            .Setup(x => x.CheckCaseLockAsync(123, It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new InvalidOperationException("Invalid operation error"));

        // Act
        IActionResult result = await this.getCaseLockInfoFunction.Run(mockRequest.Object, 123);

        // Assert
        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseLockInfo function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseLockInfo function encountered an error fetching case lock information for caseId [123]"));
    }

    /// <summary>
    /// Tests that the function returns an internal server error when an exception is thrown.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityError_WhenExceptionIsThrown()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        this.caseLockService
            .Setup(x => x.CheckCaseLockAsync(123, It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        IActionResult result = await this.getCaseLockInfoFunction.Run(mockRequest.Object, 123);

        // Assert
        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseLockInfo function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseLockInfo function encountered an unprocessable entity error:"));
    }

    private static Mock<HttpRequest> SetUpMockRequest()
    {
        var mockRequest = new Mock<HttpRequest>();

        // Set up a DefaultHttpContext to support setting headers
        var context = new DefaultHttpContext();
        mockRequest.Setup(r => r.HttpContext).Returns(context);
        mockRequest.Setup(r => r.Headers.Add("corelation", "1232131231"));

        return mockRequest;
    }
}
