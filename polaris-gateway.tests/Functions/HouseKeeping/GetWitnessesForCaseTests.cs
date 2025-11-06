// <copyright file="GetWitnessesForCaseTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Functions.Tests.Functions;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Functions.Functions;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.Interfaces.Model;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

/// <summary>
/// Unit tests for GetWitnessesForCase.
/// </summary>
public class GetWitnessesForCaseTests
{
    private readonly TestLogger<GetWitnessesForCase> mockLogger;
    private readonly Mock<IWitnessService> mockWitnessService;
    private readonly Mock<ICookieService> mockCookieService;
    private readonly GetWitnessesForCase sutGetWitnessesForCase;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetWitnessesForCaseTests"/> class.
    /// </summary>
    public GetWitnessesForCaseTests()
    {
        this.mockLogger = new TestLogger<GetWitnessesForCase>();
        this.mockWitnessService = new Mock<IWitnessService>();
        this.mockCookieService = new Mock<ICookieService>();

        this.sutGetWitnessesForCase = new GetWitnessesForCase(
            this.mockLogger,
            this.mockWitnessService.Object,
            this.mockCookieService.Object);
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
        IActionResult result = await this.sutGetWitnessesForCase.Run(mockRequest.Object);

        // Assert
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetWitnessesForCase function processed a request."));

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
        IActionResult result = await this.sutGetWitnessesForCase.Run(mockRequest.Object);

        // Assert
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetWitnessesForCase function processed a request."));

        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal($"Invalid or missing cmsCookies in the HSK cookie.", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that a valid request returns an Ok result with a success status.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ReturnsOkResult_WhenValidRequestProvidedAndServiceCallIsSuccessful()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";

        var expectedWitnesses = new WitnessesResponse
        {
            Witnesses = new List<Witness>()
            {
                new (123, 343, "Joe", "SMITH"),
                new (123, 432, "Bob", "Jackson"),
            },
        };

        // Mock the cookie service to return valid validation
        this.mockCookieService.Setup(service => service.ValidateCookies(It.IsAny<HttpRequest>())).Returns((true, null, 123));
        this.mockCookieService.Setup(service => service.GetCmsCookies(It.IsAny<HttpRequest>())).Returns("cookies");
        this.mockCookieService.Setup(service => service.GetCmsToken(It.IsAny<HttpRequest>())).Returns("token");
        this.mockCookieService.Setup(service => service.GetCaseId(It.IsAny<HttpRequest>())).Returns("321");

        // Mock the witness service to return the expected response
        this.mockWitnessService
            .Setup(svc => svc.GetWitnessesForCaseAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(expectedWitnesses);

        // Act
        IActionResult result = await this.sutGetWitnessesForCase.Run(httpRequest);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.IsType<WitnessesResponse>(okResult.Value);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetWitnessesForCase function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [321] GetWitnessesForCase function completed"));

        this.mockCookieService.Verify(svc => svc.ValidateCookies(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCmsCookies(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCmsToken(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCaseId(It.IsAny<HttpRequest>()), Times.Once);
        this.mockWitnessService.Verify(
            svc => svc.GetWitnessesForCaseAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()), Times.Once);
    }

    /// <summary>
    /// Tests that when an invalid operation exception is thrown, it is logged as error.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityError_WhenInvalidOperationExceptionIsThrown()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";

        var expectedWitnesses = new WitnessesResponse
        {
            Witnesses = new List<Witness>()
            {
                new (123, 343, "Joe", "SMITH"),
                new (123, 432, "Bob", "Jackson"),
            },
        };

        // Mock the cookie service to return valid validation
        this.mockCookieService.Setup(service => service.ValidateCookies(It.IsAny<HttpRequest>())).Returns((true, null, 123));
        this.mockCookieService.Setup(service => service.GetCmsCookies(It.IsAny<HttpRequest>())).Returns("cookies");
        this.mockCookieService.Setup(service => service.GetCmsToken(It.IsAny<HttpRequest>())).Returns("token");
        this.mockCookieService.Setup(service => service.GetCaseId(It.IsAny<HttpRequest>())).Returns("321");

        // Mock the witness service to return the expected response
        this.mockWitnessService
            .Setup(svc => svc.GetWitnessesForCaseAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new InvalidOperationException("Invalid operation error."));

        // Act
        IActionResult result = await this.sutGetWitnessesForCase.Run(httpRequest);

        // Assert
        ObjectResult objectResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetWitnessesForCase function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
              log.LogLevel == LogLevel.Error &&
              log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetWitnessesForCase function encountered an invalid operation error: Invalid operation error"));

        this.mockCookieService.Verify(svc => svc.ValidateCookies(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCmsCookies(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCmsToken(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCaseId(It.IsAny<HttpRequest>()), Times.Once);
        this.mockWitnessService.Verify(
            svc => svc.GetWitnessesForCaseAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()), Times.Once);
    }

    /// <summary>
    /// Tests that when UnauthorizedAccessException exception is thrown by the API, it is handled and logged as error.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnauthorizedError_WhenUnauthorizedAccessExceptionIsThrown()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";

        var expectedWitnesses = new WitnessesResponse
        {
            Witnesses = new List<Witness>()
            {
                new (123, 343, "Joe", "SMITH"),
                new (123, 432, "Bob", "Jackson"),
            },
        };

        // Mock the cookie service to return valid validation
        this.mockCookieService.Setup(service => service.ValidateCookies(It.IsAny<HttpRequest>())).Returns((true, null, 123));
        this.mockCookieService.Setup(service => service.GetCmsCookies(It.IsAny<HttpRequest>())).Returns("cookies");
        this.mockCookieService.Setup(service => service.GetCmsToken(It.IsAny<HttpRequest>())).Returns("token");
        this.mockCookieService.Setup(service => service.GetCaseId(It.IsAny<HttpRequest>())).Returns("321");

        // Mock the witness service to return the expected response
        this.mockWitnessService
            .Setup(svc => svc.GetWitnessesForCaseAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new UnauthorizedAccessException("Unauthorized"));

        // Act
        IActionResult result = await this.sutGetWitnessesForCase.Run(httpRequest);

        // Assert
        UnauthorizedObjectResult unauthorizedAccessResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedAccessResult.StatusCode);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetWitnessesForCase function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
             log.LogLevel == LogLevel.Error &&
             log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetWitnessesForCase function encountered an unauthorized access error: Unauthorized"));

        this.mockCookieService.Verify(svc => svc.ValidateCookies(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCmsCookies(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCmsToken(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCaseId(It.IsAny<HttpRequest>()), Times.Once);
        this.mockWitnessService.Verify(
            svc => svc.GetWitnessesForCaseAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()), Times.Once);
    }
}
