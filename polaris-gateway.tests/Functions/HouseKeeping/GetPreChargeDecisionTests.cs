// <copyright file="GetPreChargeDecisionTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions.HouseKeeping;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Common.Constants;
using Common.Dto.Request;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Functions.HouseKeeping;
using Xunit;
using ApiClient = Cps.MasterDataService.Infrastructure.ApiClient;

/// <summary>
/// Unit tests for GetPreChargeDecision.
/// </summary>
public class GetPreChargeDecisionTests
{
    private readonly TestLogger<GetPreChargeDecision> mockLogger;
    private readonly Mock<ICommunicationService> mockCommunicationService;
    private readonly GetPreChargeDecision sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPreChargeDecisionTests"/> class.
    /// </summary>
    public GetPreChargeDecisionTests()
    {
        this.mockLogger = new TestLogger<GetPreChargeDecision>();
        this.mockCommunicationService = new Mock<ICommunicationService>();
        this.sut = new GetPreChargeDecision(this.mockLogger, this.mockCommunicationService.Object);
    }

    /// <summary>
    /// Tests that the <see cref="GetPreChargeDecision.GetPreChargeDecision(ILogger{GetPreChargeDecision}, ICaseHistoryEventProvider, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();
        var expectedResponse = new ApiClient.PreChargeDecisionOutcome
        {
            CaseId = 1212,
        };

        this.mockCommunicationService.Setup(c => c.GetPreChargeDecisionCaseHistoryEventDetailsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(expectedResponse);

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId);

        // Assert
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedResponse);
        Assert.Contains(this.mockLogger.Logs, log =>
          log.LogLevel == LogLevel.Information &&
          log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] GetPreChargeDecision function completed"));
    }

    /// <summary>
    /// Tests that the <see cref="GetPreChargeDecision.GetPreChargeDecision(ILogger{GetPreChargeDecision}, ICaseHistoryEventProvider, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_GetPcdRequestOverviewReturnsNull_ReturnsUnprocessableEntity()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();

        this.mockCommunicationService.Setup(c => c.GetPreChargeDecisionCaseHistoryEventDetailsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync((ApiClient.PreChargeDecisionOutcome)null!);

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId);

        // Assert
        result.Should().BeOfType<UnprocessableEntityObjectResult>();
        Assert.Contains(this.mockLogger.Logs, log =>
         log.LogLevel == LogLevel.Error &&
         log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] GetPreChargeDecision function failed"));
    }

    /// <summary>
    /// Tests that the <see cref="GetPreChargeDecision.GetPreChargeDecision(ILogger{GetPreChargeDecision}, ICaseHistoryEventProvider, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsInvalidOperationException_ReturnsUnprocessableEntity()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();

        this.mockCommunicationService.Setup(c => c.GetPreChargeDecisionCaseHistoryEventDetailsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
                    .ThrowsAsync(new InvalidOperationException("Test Exception"));

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId);

        // Assert
        result.Should().BeOfType<UnprocessableEntityObjectResult>().Which.Value.Should().Be("Test Exception");
    }

    /// <summary>
    /// Tests that the <see cref="GetPreChargeDecision.GetPreChargeDecision(ILogger{GetPreChargeDecision}, ICaseHistoryEventProvider, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsUnauthorizedAccessException_ReturnsUnauthorized()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();

        this.mockCommunicationService.Setup(c => c.GetPreChargeDecisionCaseHistoryEventDetailsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new UnauthorizedAccessException("Unauthorized"));

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>().Which.Value.Should().Be("GetPreChargeDecision error: Unauthorized");
        Assert.Contains(this.mockLogger.Logs, log =>
                 log.LogLevel == LogLevel.Error &&
                 log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetPreChargeDecision function encountered UnauthorizedAccess Exception"));
    }

    /// <summary>
    /// Tests that the <see cref="GetPreChargeDecision.GetPreChargeDecision(ILogger{GetPreChargeDecision}, ICaseHistoryEventProvider, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsGeneralException_ReturnsInternalServerError()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();

        this.mockCommunicationService.Setup(c => c.GetPreChargeDecisionCaseHistoryEventDetailsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
               .ThrowsAsync(new Exception("General error"));

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId);

        // Assert
        result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
        Assert.Contains(this.mockLogger.Logs, log =>
                    log.LogLevel == LogLevel.Error &&
                    log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetPreChargeDecision function encountered an error"));
    }

    private static HttpRequest CreateHttpRequest(string body = "")
    {
        var context = new DefaultHttpContext();
        HttpRequest request = context.Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        return request;
    }
}
