// <copyright file="GetPcdReviewDetailsTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions.HouseKeeping;

using System;
using System.Text;
using System.Threading;
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
using Common.Dto.Response.HouseKeeping;
using Common.Dto.Request;
using Common.Exceptions;
using ApiClient = Cps.MasterDataService.Infrastructure.ApiClient;

/// <summary>
/// Unit tests for GetPcdReviewDetails.
/// </summary>
public class GetPcdReviewDetailsTests
{
    private readonly TestLogger<GetPcdReviewDetails> mockLogger;
    private readonly Mock<ICommunicationService> communicationService;
    private readonly GetPcdReviewDetails sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPcdReviewDetailsTests"/> class.
    /// </summary>
    public GetPcdReviewDetailsTests()
    {
        this.mockLogger = new TestLogger<GetPcdReviewDetails>();
        this.communicationService = new Mock<ICommunicationService>();

        this.sut = new GetPcdReviewDetails(this.mockLogger, this.communicationService.Object);
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdReviewDetails.Run"/> method returns an <see cref="OkObjectResult"/> when the request is valid.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        int caseId = 123;
        int historyId = 456;
        HttpRequest httpRequest = CreateHttpRequest();
        var expectedResponse = new PcdReviewDetailResponse
        {
            PreChargeDecisionAnalysisOutcome = new ApiClient.PreChargeDecisionAnalysisOutcome
            {
                CaseId = caseId,
                Id = historyId,
            },
        };

        this.communicationService.Setup(c => c.GetPcdReviewDetailAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId, historyId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedResponse);
        Assert.Contains(this.mockLogger.Logs, log =>
          log.LogLevel == LogLevel.Information &&
          log.Message != null && log.Message.Contains("Milestone: caseId") && log.Message.Contains("GetPcdReviewDetails function completed"));
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdReviewDetails.Run"/> method returns an <see cref="UnprocessableEntityObjectResult"/> when the result is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_GetPcdReviewDetailsReturnsNull_ReturnsUnprocessableEntity()
    {
        // Arrange
        int caseId = 123;
        int historyId = 456;
        HttpRequest httpRequest = CreateHttpRequest();

        this.communicationService.Setup(c => c.GetPcdReviewDetailAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PcdReviewDetailResponse)null!);

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId, historyId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnprocessableEntityObjectResult>();
        Assert.Contains(this.mockLogger.Logs, log =>
         log.LogLevel == LogLevel.Error &&
         log.Message != null && log.Message.Contains("GetPcdReviewDetails function failed"));
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdReviewDetails.Run"/> method returns an <see cref="UnprocessableEntityObjectResult"/> when a BadRequestException is thrown.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsBadRequestException_ReturnsBadRequest()
    {
        // Arrange
        int caseId = 123;
        int historyId = 456;
        HttpRequest httpRequest = CreateHttpRequest();

        this.communicationService.Setup(c => c.GetPcdReviewDetailAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new BadRequestException("Invalid request", "caseId"));

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId, historyId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which.Value.Should().Be("Invalid request (Parameter 'caseId')");
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdReviewDetails.Run"/> method returns an <see cref="UnprocessableEntityObjectResult"/> when an InvalidOperationException is thrown.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsInvalidOperationException_ReturnsUnprocessableEntity()
    {
        // Arrange
        int caseId = 123;
        int historyId = 456;
        HttpRequest httpRequest = CreateHttpRequest();

        this.communicationService.Setup(c => c.GetPcdReviewDetailAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new InvalidOperationException("Test Exception"));

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId, historyId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnprocessableEntityObjectResult>().Which.Value.Should().Be("Test Exception");
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdReviewDetails.Run"/> method returns an <see cref="UnprocessableEntityObjectResult"/> when a NotSupportedException is thrown.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsNotSupportedException_ReturnsUnprocessableEntity()
    {
        // Arrange
        int caseId = 123;
        int historyId = 456;
        HttpRequest httpRequest = CreateHttpRequest();

        this.communicationService.Setup(c => c.GetPcdReviewDetailAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotSupportedException("Unsupported"));

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId, historyId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnprocessableEntityObjectResult>().Which.Value.Should().Be("GetPcdReviewDetails error: Unsupported");
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains("unsupported content type"));
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdReviewDetails.Run"/> method returns an <see cref="UnauthorizedObjectResult"/> when an UnauthorizedAccessException is thrown.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsUnauthorizedAccessException_ReturnsUnauthorized()
    {
        // Arrange
        int caseId = 123;
        int historyId = 456;
        HttpRequest httpRequest = CreateHttpRequest();

        this.communicationService.Setup(c => c.GetPcdReviewDetailAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()))
           .ThrowsAsync(new UnauthorizedAccessException("Unauthorized"));

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId, historyId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>().Which.Value.Should().Be("GetPcdReviewDetails error: Unauthorized");
        Assert.Contains(this.mockLogger.Logs, log =>
                 log.LogLevel == LogLevel.Error &&
                 log.Message != null && log.Message.Contains("UnauthorizedAccess Exception"));
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdReviewDetails.Run"/> method returns an <see cref="StatusCodeResult"/> with 500 when a general Exception is thrown.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsGeneralException_ReturnsInternalServerError()
    {
        // Arrange
        int caseId = 123;
        int historyId = 456;
        HttpRequest httpRequest = CreateHttpRequest();

        this.communicationService.Setup(c => c.GetPcdReviewDetailAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()))
             .ThrowsAsync(new Exception("General error"));

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId, historyId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
        Assert.Contains(this.mockLogger.Logs, log =>
                    log.LogLevel == LogLevel.Error &&
                    log.Message != null && log.Message.Contains("encountered an error"));
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdReviewDetails.Run"/> method returns a BadRequestObjectResult when caseId is invalid.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Run_InvalidCaseId_ReturnsBadRequest(int invalidCaseId)
    {
        // Arrange
        int historyId = 456;
        HttpRequest httpRequest = CreateHttpRequest();

        // Act
        IActionResult result = await this.sut.Run(httpRequest, invalidCaseId, historyId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which.Value.Should().Be($"{LoggingConstants.HskUiLogPrefix} Invalid case Id. It should be an integer.");
        this.communicationService.Verify(c => c.GetPcdReviewDetailAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdReviewDetails.Run"/> method correctly passes the CancellationToken to the service.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_PassesCancellationToken_ToService()
    {
        // Arrange
        int caseId = 123;
        int historyId = 456;
        HttpRequest httpRequest = CreateHttpRequest();
        var customCancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token;
        var expectedResponse = new PcdReviewDetailResponse();

        this.communicationService.Setup(c => c.GetPcdReviewDetailAsync(caseId, historyId, It.IsAny<CmsAuthValues>(), customCancellationToken))
            .ReturnsAsync(expectedResponse);

        // Act
        IActionResult result = await this.sut.Run(httpRequest, caseId, historyId, customCancellationToken);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        this.communicationService.Verify(c => c.GetPcdReviewDetailAsync(caseId, historyId, It.IsAny<CmsAuthValues>(), customCancellationToken), Times.Once);
    }

    private static HttpRequest CreateHttpRequest(string body = "")
    {
        var context = new DefaultHttpContext();
        HttpRequest request = context.Request;
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        return request;
    }
}
