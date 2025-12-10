// <copyright file="GetCaseHistoryEventTests.cs" company="TheCrownProsecutionService">
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
using System.Collections.Generic;
using Common.Constants;
using System.IO;
using ApiClient = Cps.MasterDataService.Infrastructure.ApiClient;
using Common.Dto.Request;

/// <summary>
/// Unit tests for GetCaseHistoryEvent.
/// </summary>
public class GetCaseHistoryEventTests
{
    private readonly TestLogger<GetCaseHistoryEvent> mockLogger;
    private readonly Mock<ICommunicationService> communicationService;
    private readonly GetCaseHistoryEvent sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCaseHistoryEventTests"/> class.
    /// </summary>
    public GetCaseHistoryEventTests()
    {
        this.mockLogger = new TestLogger<GetCaseHistoryEvent>();
        this.communicationService = new Mock<ICommunicationService>();
        this.sut = new GetCaseHistoryEvent(this.mockLogger, this.communicationService.Object);
    }

    /// <summary>
    /// Tests that the <see cref="GetCaseHistoryEvent.GetCaseHistoryEvent(ILogger{GetCaseHistoryEvent}, ICaseHistoryEventProvider, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();
        var expectedResponse = new List<ApiClient.HistoryEvent>
        {
            new ApiClient.HistoryEvent
            {
                AuthorOrVenue = "Nag",
                Date = "01/02/2022",
                Name = "name",
            },
        };

        this.communicationService.Setup(c => c.GetHistoryEventsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(expectedResponse);

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId);

        // Assert
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedResponse);
        Assert.Contains(this.mockLogger.Logs, log =>
          log.LogLevel == LogLevel.Information &&
          log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] GetCaseHistoryEvent function completed"));
    }

    /// <summary>
    /// Tests that the <see cref="GetCaseHistoryEvent.GetCaseHistoryEvent(ILogger{GetCaseHistoryEvent}, ICaseHistoryEventProvider, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_GetPcdRequestOverviewReturnsNull_ReturnsProcessableEntity()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();

        this.communicationService.Setup(c => c.GetHistoryEventsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync((List<ApiClient.HistoryEvent>)null!);

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId);

        // Assert
        result.Should().BeOfType<UnprocessableEntityObjectResult>();
        Assert.Contains(this.mockLogger.Logs, log =>
         log.LogLevel == LogLevel.Error &&
         log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] GetCaseHistoryEvent function failed"));
    }

    /// <summary>
    /// Tests that the <see cref="GetCaseHistoryEvent.GetCaseHistoryEvent(ILogger{GetCaseHistoryEvent}, ICaseHistoryEventProvider, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsInvalidOperationException_ReturnsUnprocessableEntity()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();

        this.communicationService.Setup(c => c.GetHistoryEventsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
                   .ThrowsAsync(new InvalidOperationException("Test Exception"));

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId);

        // Assert
        result.Should().BeOfType<UnprocessableEntityObjectResult>().Which.Value.Should().Be("Test Exception");
    }

    /// <summary>
    /// Tests that the <see cref="GetCaseHistoryEvent.GetCaseHistoryEvent(ILogger{GetCaseHistoryEvent}, ICaseHistoryEventProvider, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsUnauthorizedAccessException_ReturnsUnauthorized()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();

        this.communicationService.Setup(c => c.GetHistoryEventsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
           .ThrowsAsync(new UnauthorizedAccessException("Unauthorized"));

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>().Which.Value.Should().Be("GetCaseHistoryEvent error: Unauthorized");
        Assert.Contains(this.mockLogger.Logs, log =>
                 log.LogLevel == LogLevel.Error &&
                 log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseHistoryEvent function encountered UnauthorizedAccess Exception"));
    }

    /// <summary>
    /// Tests that the <see cref="GetCaseHistoryEvent.GetCaseHistoryEvent(ILogger{GetCaseHistoryEvent}, ICaseHistoryEventProvider, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsGeneralException_ReturnsInternalServerError()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();

        this.communicationService.Setup(c => c.GetHistoryEventsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
             .ThrowsAsync(new Exception("General error"));

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId);

        // Assert
        result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
        Assert.Contains(this.mockLogger.Logs, log =>
                    log.LogLevel == LogLevel.Error &&
                    log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseHistoryEvent function encountered an error"));
    }

    private static HttpRequest CreateHttpRequest(string body = "")
    {
        var context = new DefaultHttpContext();
        HttpRequest request = context.Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        return request;
    }
}
