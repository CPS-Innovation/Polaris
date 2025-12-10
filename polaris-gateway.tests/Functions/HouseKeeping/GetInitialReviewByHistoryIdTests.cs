// <copyright file="GetInitialReviewByHistoryIdTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions.HouseKeeping;

using System;
using System.Text;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using PolarisGateway.Functions.HouseKeeping;
using Xunit;
using Common.Constants;
using System.IO;
using ApiClient = Cps.MasterDataService.Infrastructure.ApiClient;
using Common.Dto.Request;

/// <summary>
/// Unit tests for GetInitialReviewByHistoryId.
/// </summary>
public class GetInitialReviewByHistoryIdTests
{
    private readonly TestLogger<GetInitialReviewByHistoryId> mockLogger;
    private readonly Mock<ICommunicationService> communicationService;
    private readonly GetInitialReviewByHistoryId sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetInitialReviewByHistoryIdTests"/> class.
    /// </summary>
    public GetInitialReviewByHistoryIdTests()
    {
        this.mockLogger = new TestLogger<GetInitialReviewByHistoryId>();
        this.communicationService = new Mock<ICommunicationService>();
        
        this.sut = new GetInitialReviewByHistoryId(this.mockLogger, this.communicationService.Object);
    }

    /// <summary>
    /// Tests that the <see cref="GetInitialReviewByHistoryId.GetInitialReviewByHistoryId(ILogger{GetInitialReviewByHistoryId}, ICaseHistoryEventProvider, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();
        var expectedResponse = new ApiClient.PreChargeDecisionAnalysisOutcome
        {
            CaseId = 1212,
        };

        this.communicationService.Setup(c => c.GetInitialReviewByHistoryIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(expectedResponse);

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId, 121212);

        // Assert
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedResponse);
        Assert.Contains(this.mockLogger.Logs, log =>
          log.LogLevel == LogLevel.Information &&
          log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] GetInitialReviewByHistoryId function completed"));
    }

    /// <summary>
    /// Tests that the <see cref="GetInitialReviewByHistoryId.GetInitialReviewByHistoryId(ILogger{GetInitialReviewByHistoryId}, ICaseHistoryEventProvider, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_GetPcdRequestOverviewReturnsNull_ReturnsUnprocessableEntity()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();

        this.communicationService.Setup(c => c.GetInitialReviewByHistoryIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync((ApiClient.PreChargeDecisionAnalysisOutcome)null!);

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId, 121212);

        // Assert
        result.Should().BeOfType<UnprocessableEntityObjectResult>();
        Assert.Contains(this.mockLogger.Logs, log =>
         log.LogLevel == LogLevel.Error &&
         log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] GetInitialReviewByHistoryId function failed"));
    }

    /// <summary>
    /// Tests that the <see cref="GetInitialReviewByHistoryId.GetInitialReviewByHistoryId(ILogger{GetInitialReviewByHistoryId}, ICaseHistoryEventProvider, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsInvalidOperationException_ReturnsUnprocessableEntity()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();

        this.communicationService.Setup(c => c.GetInitialReviewByHistoryIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
                   .ThrowsAsync(new InvalidOperationException("Test Exception"));

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId, 121212);

        // Assert
        result.Should().BeOfType<UnprocessableEntityObjectResult>().Which.Value.Should().Be("Test Exception");
    }

    /// <summary>
    /// Tests that the <see cref="GetInitialReviewByHistoryId.GetInitialReviewByHistoryId(ILogger{GetInitialReviewByHistoryId}, ICaseHistoryEventProvider, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsUnauthorizedAccessException_ReturnsUnauthorized()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();

        this.communicationService.Setup(c => c.GetInitialReviewByHistoryIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
           .ThrowsAsync(new UnauthorizedAccessException("Unauthorized"));

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId, 121212);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>().Which.Value.Should().Be("GetInitialReviewByHistoryId error: Unauthorized");
        Assert.Contains(this.mockLogger.Logs, log =>
                 log.LogLevel == LogLevel.Error &&
                 log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetInitialReviewByHistoryId function encountered UnauthorizedAccess Exception"));
    }

    /// <summary>
    /// Tests that the <see cref="GetInitialReviewByHistoryId.GetInitialReviewByHistoryId(ILogger{GetInitialReviewByHistoryId}, ICaseHistoryEventProvider, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsGeneralException_ReturnsInternalServerError()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();

        this.communicationService.Setup(c => c.GetInitialReviewByHistoryIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
             .ThrowsAsync(new Exception("General error"));

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId, 121212);

        // Assert
        result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
        Assert.Contains(this.mockLogger.Logs, log =>
                    log.LogLevel == LogLevel.Error &&
                    log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetInitialReviewByHistoryId function encountered an error"));
    }

    private static HttpRequest CreateHttpRequest(string body = "")
    {
        var context = new DefaultHttpContext();
        HttpRequest request = context.Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        return request;
    }
}
