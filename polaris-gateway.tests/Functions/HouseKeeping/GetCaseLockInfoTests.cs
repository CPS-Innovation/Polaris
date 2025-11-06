// <copyright file="GetCaseLockInfoTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Functions.Tests.Functions;

using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Cps.Fct.Hk.Ui.Interfaces;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using Cps.Fct.Hk.Ui.Interfaces.Model;
using Microsoft.Extensions.Logging;
using Cps.Fct.Hk.Ui.Functions.Functions;

/// <summary>
/// Unit tests for the <see cref="GetCaseLockInfo"/> class.
/// </summary>
public class GetCaseLockInfoTests
{
    private readonly TestLogger<GetCaseLockInfo> mockLogger;
    private readonly Mock<ICaseLockService> caseLockService;
    private readonly Mock<ICookieService> mockCookieService;
    private readonly GetCaseLockInfo getCaseLockInfoFunction;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCaseLockInfoTests"/> class.
    /// </summary>
    public GetCaseLockInfoTests()
    {
        // Initialize mocks
        this.mockLogger = new TestLogger<GetCaseLockInfo>();
        this.caseLockService = new Mock<ICaseLockService>();
        this.mockCookieService = new Mock<ICookieService>();

        // Initialize the function class
        this.getCaseLockInfoFunction = new GetCaseLockInfo(this.mockLogger, this.caseLockService.Object, this.mockCookieService.Object);
    }

    /// <summary>
    /// Tests that the function returns a bad request when cookie validation fails.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsBadRequest_WhenCookieValidationFails()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();

        // Simulate cookie validation failure (e.g., missing case ID)
        this.mockCookieService
            .Setup(x => x.ValidateCookies(mockRequest.Object))
            .Returns((false, "Invalid or missing case_id in the HSK cookie.", null));

        // Act
        IActionResult result = await this.getCaseLockInfoFunction.Run(mockRequest.Object);

        // Assert
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseLockInfo function processed a request."));

        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal($"Invalid or missing case_id in the HSK cookie.", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that the function returns a bad request when CMS cookies are missing.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsBadRequest_WhenCmsCookiesAreMissing()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();

        // Simulate cookie validation failure (e.g., missing CMS cookies)
        this.mockCookieService
            .Setup(x => x.ValidateCookies(mockRequest.Object))
            .Returns((false, "Invalid or missing cmsCookies in the HSK cookie.", null));

        // Act
        IActionResult result = await this.getCaseLockInfoFunction.Run(mockRequest.Object);

        // Assert
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseLockInfo function processed a request."));

        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal($"Invalid or missing cmsCookies in the HSK cookie.", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that the function returns a bad request when the CMS token is missing.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsBadRequest_WhenCmsTokenIsMissing()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();

        // Simulate cookie validation failure (e.g., missing CMS token)
        this.mockCookieService
            .Setup(x => x.ValidateCookies(mockRequest.Object))
            .Returns((false, "Invalid or missing cmsToken in the HSK cookie.", null));

        // Act
        IActionResult result = await this.getCaseLockInfoFunction.Run(mockRequest.Object);

        // Assert
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseLockInfo function processed a request."));

        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal($"Invalid or missing cmsToken in the HSK cookie.", badRequestResult.Value);
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

        // Simulate successful cookie validation
        this.mockCookieService
            .Setup(x => x.ValidateCookies(mockRequest.Object))
            .Returns((true, null, 123));

        this.mockCookieService
            .Setup(x => x.GetCaseId(mockRequest.Object))
            .Returns("123");

        this.mockCookieService
            .Setup(x => x.GetCmsCookies(mockRequest.Object))
            .Returns("validCmsCookies");

        this.mockCookieService
            .Setup(x => x.GetCmsToken(mockRequest.Object))
            .Returns("validCmsToken");

        var mockCaseInfo = new CaseLockedStatusResult
        {
            IsLocked = true,
        };

        // Ensure the mock returns the expected list of communications
        this.caseLockService
            .Setup(x => x.CheckCaseLockAsync(123, It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(mockCaseInfo);

        // Act
        IActionResult result = await this.getCaseLockInfoFunction.Run(mockRequest.Object);

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
        var mockRequest = new Mock<HttpRequest>();

        // Simulate successful cookie validation
        this.mockCookieService
            .Setup(x => x.ValidateCookies(mockRequest.Object))
            .Returns((true, null, 123));

        this.mockCookieService
            .Setup(x => x.GetCaseId(mockRequest.Object))
            .Returns("123");

        this.mockCookieService
            .Setup(x => x.GetCmsCookies(mockRequest.Object))
            .Returns("validCmsCookies");

        this.mockCookieService
            .Setup(x => x.GetCmsToken(mockRequest.Object))
            .Returns("validCmsToken");

        this.caseLockService
            .Setup(x => x.CheckCaseLockAsync(123, It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new InvalidOperationException("Invalid operation error"));

        // Act
        IActionResult result = await this.getCaseLockInfoFunction.Run(mockRequest.Object);

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
    public async Task Run_ReturnsInternalServerError_WhenExceptionIsThrown()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();

        // Simulate successful cookie validation
        this.mockCookieService
            .Setup(x => x.ValidateCookies(mockRequest.Object))
            .Throws(new Exception("Unexpected error"));

        this.mockCookieService
            .Setup(x => x.GetCaseId(mockRequest.Object))
            .Returns("123");

        this.mockCookieService
            .Setup(x => x.GetCmsCookies(mockRequest.Object))
            .Returns("validCmsCookies");

        this.mockCookieService
            .Setup(x => x.GetCmsToken(mockRequest.Object))
            .Returns("validCmsToken");

        // Act
        IActionResult result = await this.getCaseLockInfoFunction.Run(mockRequest.Object);

        // Assert
        StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseLockInfo function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseLockInfo function encountered an error: " +
            "Unexpected error"));
    }

    /// <summary>
    /// Tests that the function returns an internal server error when an exception is thrown.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityError_WhenExceptionIsThrown()
    {
        // Arrange
        var mockRequest = new Mock<HttpRequest>();

        // Simulate successful cookie validation
        this.mockCookieService
            .Setup(x => x.ValidateCookies(mockRequest.Object))
            .Returns((true, null, 123));

        this.mockCookieService
            .Setup(x => x.GetCaseId(mockRequest.Object))
            .Returns("123");

        this.mockCookieService
            .Setup(x => x.GetCmsCookies(mockRequest.Object))
            .Returns("validCmsCookies");

        this.mockCookieService
            .Setup(x => x.GetCmsToken(mockRequest.Object))
            .Returns("validCmsToken");

        this.caseLockService
            .Setup(x => x.CheckCaseLockAsync(123, It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        IActionResult result = await this.getCaseLockInfoFunction.Run(mockRequest.Object);

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
}
