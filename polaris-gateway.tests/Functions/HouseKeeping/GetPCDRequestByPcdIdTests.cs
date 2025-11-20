// <copyright file="GetPCDRequestByPcdIdTests.cs" company="TheCrownProsecutionService">
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
using Cps.Fct.Hk.Ui.Interfaces.Model;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using PolarisGateway.Functions.HouseKeeping;
using Xunit;
using Common.Dto.Response.HouseKeeping.Pcd;
using Common.Dto.Request;
using Common.Constants;
using System.IO;

/// <summary>
/// Unit tests for GetPCDRequestByPcdId.
/// </summary>
public class GetPcdRequestByPcdIdTests
{
    private readonly TestLogger<GetPcdRequestByPcdId> mockLogger;
    private readonly Mock<ICommunicationService> mockCommunicationService;
    private readonly GetPcdRequestByPcdId sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPcdRequestByPcdIdTests"/> class.
    /// </summary>
    public GetPcdRequestByPcdIdTests()
    {
        this.mockLogger = new TestLogger<GetPcdRequestByPcdId>();
        this.mockCommunicationService = new Mock<ICommunicationService>();

        this.sut = new GetPcdRequestByPcdId(this.mockLogger, this.mockCommunicationService.Object);
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdRequestByPcdId.GetPcdRequestByPcdId(ILogger{GetPcdRequestByPcdId}, ICommunicationService, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        int caseId = 123;
        int pcdId = 121212;
        HttpRequest httpRequest = CreateHttpRequest();
        var expectedResponse = new PcdRequestDto { };

        this.mockCommunicationService.Setup(c => c.GetPcdRequestByPcdIdAsync(caseId, pcdId, It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(expectedResponse);

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId, pcdId);

        // Assert
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedResponse);
        Assert.Contains(this.mockLogger.Logs, log =>
          log.LogLevel == LogLevel.Information &&
          log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] GetPCDRequestByPcdId function completed"));
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdRequestByPcdId.GetPcdRequestByPcdId(ILogger{GetPcdRequestByPcdId}, ICommunicationService, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_GetPcdRequestOverviewReturnsNull_ReturnsUnprocessableEntity()
    {
        // Arrange
        int caseId = 123;
        int pcdId = 1212;
        HttpRequest httpRequest = CreateHttpRequest();

        this.mockCommunicationService.Setup(c => c.GetPcdRequestByPcdIdAsync(caseId, pcdId, It.IsAny<CmsAuthValues>()))
           .ReturnsAsync((PcdRequestDto)null!);

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId, pcdId);

        // Assert
        result.Should().BeOfType<UnprocessableEntityObjectResult>();
        Assert.Contains(this.mockLogger.Logs, log =>
         log.LogLevel == LogLevel.Error &&
         log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] GetPCDRequestByPcdId function failed"));
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdRequestByPcdId.GetPcdRequestByPcdId(ILogger{GetPcdRequestByPcdId}, ICommunicationService, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsInvalidOperationException_ReturnsUnprocessableEntity()
    {
        // Arrange
        int caseId = 123;
        int pcdId = 121212;
        HttpRequest httpRequest = CreateHttpRequest();

        this.mockCommunicationService.Setup(c => c.GetPcdRequestByPcdIdAsync(caseId, pcdId, It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new InvalidOperationException("Test Exception"));

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId, pcdId);

        // Assert
        result.Should().BeOfType<UnprocessableEntityObjectResult>().Which.Value.Should().Be("Test Exception");
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdRequestByPcdId.GetPcdRequestByPcdId(ILogger{GetPcdRequestByPcdId}, ICommunicationService, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsUnauthorizedAccessException_ReturnsUnauthorized()
    {
        // Arrange
        int caseId = 123;
        int pcdId = 121212;
        HttpRequest httpRequest = CreateHttpRequest();

        this.mockCommunicationService.Setup(c => c.GetPcdRequestByPcdIdAsync(caseId, pcdId, It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new UnauthorizedAccessException("Unauthorized"));

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId, pcdId);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>().Which.Value.Should().Be("GetPCDRequestByPcdId error: Unauthorized");
        Assert.Contains(this.mockLogger.Logs, log =>
                 log.LogLevel == LogLevel.Error &&
                 log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetPCDRequestByPcdId function encountered UnauthorizedAccess Exception"));
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdRequestByPcdId.GetPcdRequestByPcdId(ILogger{GetPcdRequestByPcdId}, ICommunicationService, ICookieService)"/> method returns an <see cref="OkObjectResult"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsGeneralException_ReturnsInternalServerError()
    {
        // Arrange
        int caseId = 123;
        int pcdId = 121212;
        HttpRequest httpRequest = CreateHttpRequest();

        this.mockCommunicationService.Setup(c => c.GetPcdRequestByPcdIdAsync(caseId, pcdId, It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new Exception("General error"));

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId, pcdId);

        // Assert
        result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
        Assert.Contains(this.mockLogger.Logs, log =>
                    log.LogLevel == LogLevel.Error &&
                    log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetPCDRequestByPcdId function encountered an error"));
    }

    private static HttpRequest CreateHttpRequest(string body = "")
    {
        var context = new DefaultHttpContext();
        HttpRequest request = context.Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        return request;
    }
}
