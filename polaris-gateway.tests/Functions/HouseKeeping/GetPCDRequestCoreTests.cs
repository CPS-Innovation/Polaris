// <copyright file="GetPCDRequestCoreTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions.HouseKeeping;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using Xunit;
using PolarisGateway.Functions.HouseKeeping;
using Common.Dto.Response.HouseKeeping.Pcd;
using Common.Dto.Request;
using Common.Constants;
using System.IO;

/// <summary>
/// Unit tests for GetPCDRequestCore.
/// </summary>
public class GetPcdRequestCoreTests
{
    private readonly TestLogger<GetPcdRequestCore> mockLogger;
    private readonly Mock<ICommunicationService> mockCommunicationService;
    private readonly GetPcdRequestCore getPCDRequestCore;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPcdRequestCoreTests"/> class.
    /// </summary>
    public GetPcdRequestCoreTests()
    {
        this.mockLogger = new TestLogger<GetPcdRequestCore>();
        this.mockCommunicationService = new Mock<ICommunicationService>();
        this.getPCDRequestCore = new GetPcdRequestCore(this.mockLogger, this.mockCommunicationService.Object);
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdRequestCore.GetPcdRequestCore(ILogger{GetPcdRequestCore}, ICommunicationService, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();
        var expectedResponse = new List<PcdRequestCore> { new() };

        this.mockCommunicationService.Setup(c => c.GetPcdRequestCore(caseId, It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(expectedResponse);

        // Act
        IActionResult result = await this.getPCDRequestCore.Run(httpRequest, caseId);

        // Assert
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedResponse);
        Assert.Contains(this.mockLogger.Logs, log =>
          log.LogLevel == LogLevel.Information &&
          log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] GetPCDRequestCore function completed"));
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdRequestCore.GetPcdRequestCore(ILogger{GetPcdRequestCore}, ICommunicationService, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_GetPcdRequestCoreReturnsNull_ReturnsUnprocessableEntity()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();

        this.mockCommunicationService.Setup(c => c.GetPcdRequestCore(caseId, It.IsAny<CmsAuthValues>()))
           .ReturnsAsync((List<PcdRequestCore>)null!);

        // Act
        IActionResult result = await this.getPCDRequestCore.Run(httpRequest, caseId);

        // Assert
        result.Should().BeOfType<UnprocessableEntityObjectResult>();
        Assert.Contains(this.mockLogger.Logs, log =>
         log.LogLevel == LogLevel.Error &&
         log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] GetPCDRequestCore function failed"));
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdRequestCore.GetPcdRequestCore(ILogger{GetPcdRequestCore}, ICommunicationService, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsInvalidOperationException_ReturnsUnprocessableEntity()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();

        this.mockCommunicationService.Setup(c => c.GetPcdRequestCore(caseId, It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new InvalidOperationException("Test Exception"));

        // Act
        IActionResult result = await this.getPCDRequestCore.Run(httpRequest, caseId);

        // Assert
        result.Should().BeOfType<UnprocessableEntityObjectResult>().Which.Value.Should().Be("Test Exception");
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdRequestCore.GetPcdRequestCore(ILogger{GetPcdRequestCore}, ICommunicationService, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsUnauthorizedAccessException_ReturnsUnauthorized()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();

        this.mockCommunicationService.Setup(c => c.GetPcdRequestCore(caseId, It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new UnauthorizedAccessException("Unauthorized"));

        // Act
        IActionResult result = await this.getPCDRequestCore.Run(httpRequest, caseId);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>().Which.Value.Should().Be("GetPCDRequestCore error: Unauthorized");
        Assert.Contains(this.mockLogger.Logs, log =>
                 log.LogLevel == LogLevel.Error &&
                 log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetPCDRequestCore function encountered UnauthorizedAccess Exception"));
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdRequestCore.GetPcdRequestCore(ILogger{GetPcdRequestCore}, ICommunicationService, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsGeneralException_ReturnsInternalServerError()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();

        this.mockCommunicationService.Setup(c => c.GetPcdRequestCore(caseId, It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new Exception("General error"));

        // Act
        IActionResult result = await this.getPCDRequestCore.Run(httpRequest, caseId);

        // Assert
        result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
        Assert.Contains(this.mockLogger.Logs, log =>
                    log.LogLevel == LogLevel.Error &&
                    log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetPCDRequestCore function encountered an error"));
    }

    private static HttpRequest CreateHttpRequest(string body = "")
    {
        var context = new DefaultHttpContext();
        HttpRequest request = context.Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        request.Cookies = null;
        return request;
    }
}
