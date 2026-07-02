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
using System.Threading;
using Common.Constants;
using Common.Exceptions;

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

        // Set up cookies
        var mockCookies = new Mock<IRequestCookieCollection>();
        mockCookies.Setup(c => c.TryGetValue(It.IsAny<string>(), out It.Ref<string>.IsAny))
            .Returns(false);

        mockRequest.Setup(r => r.Cookies).Returns(mockCookies.Object);
        mockRequest.Setup(r => r.HttpContext).Returns(context);
        mockRequest.Setup(r => r.Headers.Add("corelation", "1232131231"));

        var mockCaseInfo = new CaseSummaryResponse(123, "06SC1234572", "Will", "SMITH", 2, "Hull UT");

        // Ensure the mock returns the expected list of communications
        mockCaseInfoService
            .Setup(x => x.GetCaseInfoAsync(123, It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockCaseInfo);

        // Act
        IActionResult result = await getCaseInfoFunction.Run(mockRequest.Object, 123, CancellationToken.None);

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
    public async Task Run_Returns500InternalError_WhenUnhandledExceptionIsThrown()
    {
        // Arrange
        Mock<HttpRequest> mockRequest = SetUpMockRequest();

        mockCaseInfoService
            .Setup(x => x.GetCaseInfoAsync(123, It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Internal server error"));

        // Act
        IActionResult result = await getCaseInfoFunction.Run(mockRequest.Object, 123, CancellationToken.None);

        // Assert
        StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseInfo function processed a request."));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseInfo function encountered an error"));
    }

    /// <summary>
    /// Tests that the function returns an unprocessable entity error when an InvalidOperationException is thrown.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityError_WhenInvalidOperationExceptionIsThrown()
    {
        // Arrange
        Mock<HttpRequest> mockRequest = SetUpMockRequest();

        mockCaseInfoService
            .Setup(x => x.GetCaseInfoAsync(123, It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Invalid operation error"));

        // Act
        IActionResult result = await getCaseInfoFunction.Run(mockRequest.Object, 123, CancellationToken.None);

        // Assert
        UnprocessableEntityObjectResult unprocessableResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
        Assert.Equal("Invalid operation error", unprocessableResult.Value);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseInfo function processed a request."));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Invalid operation error"));
    }

    /// <summary>
    /// Tests that the function returns an unprocessable entity error when an UnprocessableEntityException is thrown.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityError_WhenUnprocessableEntityExceptionIsThrown()
    {
        // Arrange
        Mock<HttpRequest> mockRequest = SetUpMockRequest();

        mockCaseInfoService
            .Setup(x => x.GetCaseInfoAsync(123, It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnprocessableEntityException("Unprocessable entity error"));

        // Act
        IActionResult result = await getCaseInfoFunction.Run(mockRequest.Object, 123, CancellationToken.None);

        // Assert
        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);
        Assert.Equal("Unprocessable entity error", objectResult.Value);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseInfo function processed a request."));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseInfo function encountered an unprocessable entity error: Unprocessable entity error"));
    }

    /// <summary>
    /// Tests that the function properly handles cancellation when the cancellation token is cancelled.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ProperlyCancels_WhenCancellationTokenIsCancelled()
    {
        // Arrange
        Mock<HttpRequest> mockRequest = SetUpMockRequest();
        var cancellationTokenSource = new CancellationTokenSource();

        // Set up the service to throw OperationCanceledException when called
        mockCaseInfoService
            .Setup(x => x.GetCaseInfoAsync(123, It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException("Operation was cancelled"));

        // Act & Assert - expect OperationCanceledException to propagate
        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await getCaseInfoFunction.Run(mockRequest.Object, 123, cancellationTokenSource.Token));

        // Verify the service was called
        mockCaseInfoService.Verify(
            x => x.GetCaseInfoAsync(123, It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()),
            Times.Once);

        // Verify that cancellation was logged
        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseInfo function was cancelled for caseId [123]"));
    }

    /// <summary>
    /// Tests that the cancellation token is passed through to the service layer.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_PassesCancellationToken_ToServiceLayer()
    {
        // Arrange
        Mock<HttpRequest> mockRequest = SetUpMockRequest();
        var cancellationTokenSource = new CancellationTokenSource();
        var mockCaseInfo = new CaseSummaryResponse(123, "06SC1234572", "Will", "SMITH", 2, "Hull UT");

        CancellationToken capturedToken = default;

        mockCaseInfoService
            .Setup(x => x.GetCaseInfoAsync(123, It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()))
            .Callback<int, CmsAuthValues, CancellationToken>((caseId, authValues, ct) =>
            {
                capturedToken = ct;
            })
            .ReturnsAsync(mockCaseInfo);

        // Act
        await getCaseInfoFunction.Run(mockRequest.Object, 123, cancellationTokenSource.Token);

        // Assert
        Assert.Equal(cancellationTokenSource.Token, capturedToken);

        mockCaseInfoService.Verify(
            x => x.GetCaseInfoAsync(123, It.IsAny<CmsAuthValues>(), cancellationTokenSource.Token),
            Times.Once);
    }

    private static Mock<HttpRequest> SetUpMockRequest()
    {
        var mockRequest = new Mock<HttpRequest>();

        // Set up a DefaultHttpContext to support setting headers
        var context = new DefaultHttpContext();

        // Set up cookies
        var mockCookies = new Mock<IRequestCookieCollection>();
        mockCookies.Setup(c => c.TryGetValue(It.IsAny<string>(), out It.Ref<string>.IsAny))
            .Returns(false);

        mockRequest.Setup(r => r.Cookies).Returns(mockCookies.Object);
        mockRequest.Setup(r => r.HttpContext).Returns(context);
        mockRequest.Setup(r => r.Headers.Add("corelation", "1232131231"));

        return mockRequest;
    }
}
