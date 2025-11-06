// <copyright file="GetExhibitProducersTests.cs" company="TheCrownProsecutionService">
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
/// Unit tests for GetExhibitProducers function.
/// </summary>
public class GetExhibitProducersTests
{
    private readonly TestLogger<GetExhibitProducers> mockLogger;
    private readonly Mock<ICommunicationService> mockCommunicationService;
    private readonly Mock<ICookieService> mockCookieService;
    private readonly Mock<IWitnessService> mockWitnessService;
    private readonly GetExhibitProducers sutGetExhibitProducers;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetExhibitProducersTests"/> class.
    /// </summary>
    public GetExhibitProducersTests()
    {
        this.mockLogger = new TestLogger<GetExhibitProducers>();
        this.mockCommunicationService = new Mock<ICommunicationService>();
        this.mockCookieService = new Mock<ICookieService>();
        this.mockWitnessService = new Mock<IWitnessService>();

        this.sutGetExhibitProducers = new GetExhibitProducers(
            this.mockLogger,
            this.mockCommunicationService.Object,
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
        IActionResult result = await this.sutGetExhibitProducers.Run(mockRequest.Object);

        // Assert
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetExhibitProducers function processed a request."));

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
        IActionResult result = await this.sutGetExhibitProducers.Run(mockRequest.Object);

        // Assert
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetExhibitProducers function processed a request."));

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

        var expectedProducers = new ExhibitProducersResponse
        {
            ExhibitProducers = new List<ExhibitProducer>()
            {
                new (Id: 343, "Joe SMITH", false),
                new (Id: 346, "Bob JACKSON", false),
            },
        };

        var witnesses = new WitnessesResponse
        {
            Witnesses = new List<Witness>()
            {
                new Witness(CaseId: 221, WitnessId: 34, "Jane", "Jones"),
                new Witness(CaseId: 221, WitnessId: 36, "Bill", "Ted"),
            },
        };

        this.mockCookieService.Setup(service => service.ValidateCookies(It.IsAny<HttpRequest>())).Returns((true, null, 123));
        this.mockCookieService.Setup(service => service.GetCmsCookies(It.IsAny<HttpRequest>())).Returns("cookies");
        this.mockCookieService.Setup(service => service.GetCmsToken(It.IsAny<HttpRequest>())).Returns("token");
        this.mockCookieService.Setup(service => service.GetCaseId(It.IsAny<HttpRequest>())).Returns("321");

        this.mockCommunicationService
            .Setup(x => x.GetExhibitProducersAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(expectedProducers);

        this.mockWitnessService
            .Setup(x => x.GetWitnessesForCaseAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(witnesses);

        // Act
        IActionResult result = await this.sutGetExhibitProducers.Run(httpRequest);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.IsType<ExhibitProducersResponse>(okResult.Value);

        var producers = (ExhibitProducersResponse)okResult.Value;

        Assert.NotNull(producers);
        Assert.Equal(4, producers.ExhibitProducers!.Count);
        Assert.Equal(2, producers.ExhibitProducers.Where(x => x.IsWitness).Count());
        Assert.Equal("Jane Jones", producers.ExhibitProducers.Where(x => x.IsWitness).First().Name);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetExhibitProducers function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [321] GetExhibitProducers function completed"));

        this.mockCookieService.Verify(svc => svc.ValidateCookies(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCmsCookies(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCmsToken(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCaseId(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCommunicationService.Verify(
            svc => svc.GetExhibitProducersAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()), Times.Once);
    }

    /// <summary>
    /// Tests that a valid request returns an Ok result with a success status.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ReturnsOkResult_WhenCaseHasExhibitsButNoWitnesses_ReturnsExhibitsOnly()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";

        var expectedProducers = new ExhibitProducersResponse
        {
            ExhibitProducers = new List<ExhibitProducer>()
            {
                new (Id: 343, "Joe SMITH", false),
                new (Id: 346, "Bob JACKSON", false),
            },
        };

        var witnesses = new WitnessesResponse
        {
            Witnesses = null,
        };

        this.mockCookieService.Setup(service => service.ValidateCookies(It.IsAny<HttpRequest>())).Returns((true, null, 123));
        this.mockCookieService.Setup(service => service.GetCmsCookies(It.IsAny<HttpRequest>())).Returns("cookies");
        this.mockCookieService.Setup(service => service.GetCmsToken(It.IsAny<HttpRequest>())).Returns("token");
        this.mockCookieService.Setup(service => service.GetCaseId(It.IsAny<HttpRequest>())).Returns("321");

        this.mockCommunicationService
            .Setup(x => x.GetExhibitProducersAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(expectedProducers);

        this.mockWitnessService
            .Setup(x => x.GetWitnessesForCaseAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(witnesses);

        // Act
        IActionResult result = await this.sutGetExhibitProducers.Run(httpRequest);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.IsType<ExhibitProducersResponse>(okResult.Value);

        var producers = (ExhibitProducersResponse)okResult.Value;

        Assert.NotNull(producers);
        Assert.Equal(2, producers.ExhibitProducers!.Count);
        Assert.Equal(2, producers.ExhibitProducers.Where(x => x.IsWitness == false).Count());

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetExhibitProducers function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [321] GetExhibitProducers function completed"));

        this.mockCookieService.Verify(svc => svc.ValidateCookies(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCmsCookies(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCmsToken(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCaseId(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCommunicationService.Verify(
            svc => svc.GetExhibitProducersAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()), Times.Once);
    }

    /// <summary>
    /// Tests that a valid request returns an Ok result with a success status.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ReturnsOkResult_WhenCaseHasWitnessesButNoExhibits()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";

        var expectedProducers = new ExhibitProducersResponse
        {
            ExhibitProducers = null,
        };

        var witnesses = new WitnessesResponse
        {
            Witnesses = new List<Witness>()
            {
                new Witness(CaseId: 221, WitnessId: 34, "Jane", "Jones"),
                new Witness(CaseId: 221, WitnessId: 36, "Bill", "Ted"),
            },
        };

        this.mockCookieService.Setup(service => service.ValidateCookies(It.IsAny<HttpRequest>())).Returns((true, null, 123));
        this.mockCookieService.Setup(service => service.GetCmsCookies(It.IsAny<HttpRequest>())).Returns("cookies");
        this.mockCookieService.Setup(service => service.GetCmsToken(It.IsAny<HttpRequest>())).Returns("token");
        this.mockCookieService.Setup(service => service.GetCaseId(It.IsAny<HttpRequest>())).Returns("321");

        this.mockCommunicationService
            .Setup(x => x.GetExhibitProducersAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(expectedProducers);

        this.mockWitnessService
            .Setup(x => x.GetWitnessesForCaseAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(witnesses);

        // Act
        IActionResult result = await this.sutGetExhibitProducers.Run(httpRequest);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.IsType<ExhibitProducersResponse>(okResult.Value);

        var producers = (ExhibitProducersResponse)okResult.Value;

        Assert.NotNull(producers);
        Assert.Equal(2, producers.ExhibitProducers!.Count);
        Assert.Equal(2, producers.ExhibitProducers.Where(x => x.IsWitness == true).Count());

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetExhibitProducers function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [321] GetExhibitProducers function completed"));

        this.mockCookieService.Verify(svc => svc.ValidateCookies(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCmsCookies(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCmsToken(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCaseId(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCommunicationService.Verify(
            svc => svc.GetExhibitProducersAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()), Times.Once);
    }

    /// <summary>
    /// Tests that when an invalid operation exception is thrown, it is handled and logged as error.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityError_WhenInvalidOperationExceptionIsThrown()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";
        var cmsAuthValues = new CmsAuthValues("validCmsToken", "validCmsCookies");

        var expectedProducers = new ExhibitProducersResponse
        {
            ExhibitProducers = new List<ExhibitProducer>()
            {
                new (Id: 343, "Joe SMITH", false),
                new (Id: 343, "Bob JACKSON", false),
            },
        };

        this.mockCookieService.Setup(service => service.ValidateCookies(It.IsAny<HttpRequest>())).Returns((true, null, 123));
        this.mockCookieService.Setup(service => service.GetCmsCookies(It.IsAny<HttpRequest>())).Returns("cookies");
        this.mockCookieService.Setup(service => service.GetCmsToken(It.IsAny<HttpRequest>())).Returns("token");
        this.mockCookieService.Setup(service => service.GetCaseId(It.IsAny<HttpRequest>())).Returns("321");

        this.mockCommunicationService
            .Setup(x => x.GetExhibitProducersAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new InvalidOperationException("Invalid operation error"));

        // Act
        IActionResult result = await this.sutGetExhibitProducers.Run(httpRequest);

        // Assert
        ObjectResult objectResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetExhibitProducers function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetExhibitProducers function encountered an invalid operation error: Invalid operation error"));

        this.mockCookieService.Verify(svc => svc.ValidateCookies(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCmsCookies(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCmsToken(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCaseId(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCommunicationService.Verify(
            svc => svc.GetExhibitProducersAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()), Times.Once);
    }

    /// <summary>
    /// Tests that when UnauthorizedAccessException exception is thrown by the API, it is handled and logged as error.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityError_WhenUnauthorizedAccessExceptionIsThrown()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";
        var cmsAuthValues = new CmsAuthValues("validCmsToken", "validCmsCookies");

        var expectedProducers = new ExhibitProducersResponse
        {
            ExhibitProducers = new List<ExhibitProducer>()
            {
                new (Id: 343, "Joe SMITH", false),
                new (Id: 343, "Bob JACKSON", false),
            },
        };

        this.mockCookieService.Setup(service => service.ValidateCookies(It.IsAny<HttpRequest>())).Returns((true, null, 123));
        this.mockCookieService.Setup(service => service.GetCmsCookies(It.IsAny<HttpRequest>())).Returns("cookies");
        this.mockCookieService.Setup(service => service.GetCmsToken(It.IsAny<HttpRequest>())).Returns("token");
        this.mockCookieService.Setup(service => service.GetCaseId(It.IsAny<HttpRequest>())).Returns("321");

        this.mockCommunicationService
          .Setup(x => x.GetExhibitProducersAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
          .ThrowsAsync(new UnauthorizedAccessException("Unauthorized"));

        // Act
        IActionResult result = await this.sutGetExhibitProducers.Run(httpRequest);

        // Assert
        UnauthorizedObjectResult unauthorizedAccessResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedAccessResult.StatusCode);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetExhibitProducers function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetExhibitProducers function encountered an unauthorized access error: Unauthorized"));

        this.mockCookieService.Verify(svc => svc.ValidateCookies(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCmsCookies(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCmsToken(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCookieService.Verify(svc => svc.GetCaseId(It.IsAny<HttpRequest>()), Times.Once);
        this.mockCommunicationService.Verify(
            svc => svc.GetExhibitProducersAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()), Times.Once);
    }
}
