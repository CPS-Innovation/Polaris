// <copyright file="GetCaseInfoTests.cs" company="TheCrownProsecutionService">
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
using PolarisGateway.Functions.HouseKeeping;
using Common.Dto.Response.HouseKeeping;
using Common.Dto.Request;
using System;
using Common.Constants;

/// <summary>
/// Unit tests for the <see cref="GetCaseInfo"/> class.
/// </summary>
public class GetCaseInfoTests
{
    private readonly TestLogger<GetCaseInfo> mockLogger;
    private readonly Mock<ICaseInfoService> mockCaseInfoService;
    private readonly GetCaseInfo getCaseInfoFunction;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCaseInfoTests"/> class.
    /// </summary>
    public GetCaseInfoTests()
    {
        // Initialize mocks
        mockLogger = new TestLogger<GetCaseInfo>();
        mockCaseInfoService = new Mock<ICaseInfoService>();

        // Initialize the function class
        getCaseInfoFunction = new GetCaseInfo(mockLogger, mockCaseInfoService.Object);
    }


    /// <summary>
    /// Tests that the function returns an OK result when a valid request is provided.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsOkResult_WhenValidRequestProvided()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();

        // Set up a DefaultHttpContext to support setting headers
        var context = new DefaultHttpContext();
        mockRequest.Setup(r => r.HttpContext).Returns(context);
        mockRequest.Setup(r => r.Headers.Add("corelation", "1232131231"));

        var mockCaseInfo = new CaseSummaryResponse(123, "06SC1234572", "Will", "SMITH", 2, "Hull UT");

        // Ensure the mock returns the expected list of communications
        mockCaseInfoService
            .Setup(x => x.GetCaseInfoAsync(123, It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(mockCaseInfo);

        // Act
        IActionResult result = await getCaseInfoFunction.Run(mockRequest.Object, 123);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);

        // Check that the communications are not null
        Assert.NotNull(okResult.Value);

        // Ensure that the correct communications were returned
        var caseInfo = Assert.IsType<CaseSummaryResponse>(okResult.Value);
        Assert.Equal(mockCaseInfo, caseInfo);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseInfo function processed a request."));

        Assert.Contains(mockLogger.Logs, log =>
           log.LogLevel == LogLevel.Information &&
           log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseInfo function call returned unitName [Hull UT] for caseId [123]"));
    }

    /// <summary>
    /// Tests that the function returns an unprocessable entity error when an invalid operation exception is thrown.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityError_WhenInvalidOperationExceptionIsThrown()
    {
        // Arrange
        Mock<HttpRequest> mockRequest = SetUpMockRequest();

        mockCaseInfoService
            .Setup(x => x.GetCaseInfoAsync(123, It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new InvalidOperationException("Invalid operation error"));

        // Act
        IActionResult result = await getCaseInfoFunction.Run(mockRequest.Object, 123);

        // Assert
        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseInfo function processed a request."));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseInfo function encountered an unprocessable entity error: " +
            "GetCaseInfo function encountered an error fetching case information for caseId [123]"));
    }

    /// <summary>
    /// Tests that the function returns an internal server error when an exception is thrown.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityError_WhenExceptionIsThrown()
    {
        // Arrange
        Mock<HttpRequest> mockRequest = SetUpMockRequest();

        mockCaseInfoService
            .Setup(x => x.GetCaseInfoAsync(123, It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        IActionResult result = await getCaseInfoFunction.Run(mockRequest.Object, 123);

        // Assert
        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseInfo function processed a request."));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseInfo function encountered an unprocessable entity error: " +
            "GetCaseInfo function encountered an error fetching case information for caseId [123]"));
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
