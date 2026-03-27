// <copyright file="GetPcdReviewCoreTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions.HouseKeeping;

using System;
using System.Collections.Generic;
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
using Common.Enums;
using ApiClient = Cps.MasterDataService.Infrastructure.ApiClient;

/// <summary>
/// Unit tests for GetPcdReviewCore.
/// </summary>
public class GetPcdReviewCoreTests
{
    private readonly TestLogger<GetPcdReviewCore> mockLogger;
    private readonly Mock<ICommunicationService> communicationService;
    private readonly GetPcdReviewCore sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPcdReviewCoreTests"/> class.
    /// </summary>
    public GetPcdReviewCoreTests()
    {
        this.mockLogger = new TestLogger<GetPcdReviewCore>();
        this.communicationService = new Mock<ICommunicationService>();

        this.sut = new GetPcdReviewCore(this.mockLogger, this.communicationService.Object);
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdReviewCore.Run"/> method returns an <see cref="OkObjectResult"/> when the request is valid.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        int caseId = 123;
        const string caseUrn = "54KR7689125";
        HttpRequest httpRequest = CreateHttpRequest();
        var expectedResponse = new List<PcdReviewCoreResponseDto>
        {
            new() { Id = "4472765", Type = PcdReviewCoreType.InitialReview, Date = "27/01/2026" },
            new() { Id = "4472772", Type = PcdReviewCoreType.PreChargeDecisionAnalysis, Date = "27/01/2026" },
        };

        this.communicationService.Setup(c => c.GetPcdReviewCoreAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        IActionResult result = await this.sut.Run(httpRequest, "54KR7689125", caseId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedResponse);
        Assert.Contains(this.mockLogger.Logs, log =>
          log.LogLevel == LogLevel.Information &&
          log.Message != null && log.Message.Contains("Milestone: caseId") && log.Message.Contains("GetPcdReviewCore function completed"));
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdReviewCore.Run"/> method returns an <see cref="OkObjectResult"/> when the response contains Early Advice.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ResponseContainsEarlyAdvice_ReturnsOkResult()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();
        var expectedResponse = new List<PcdReviewCoreResponseDto>
        {
            new() { Id = "4472751", Type = PcdReviewCoreType.EarlyAdvice, Date = "27/01/2026" },
            new() { Id = "4472765", Type = PcdReviewCoreType.InitialReview, Date = "27/01/2026" },
        };

        this.communicationService.Setup(c => c.GetPcdReviewCoreAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        IActionResult result = await this.sut.Run(httpRequest, "54KR7689125", caseId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedResponse);
        var okResult = result as OkObjectResult;
        var response = okResult?.Value as IReadOnlyCollection<PcdReviewCoreResponseDto>;
        response.Should().NotBeNull();
        response!.Should().HaveCount(2);

        var responseList = new List<PcdReviewCoreResponseDto>(response);
        responseList[0].Type.Should().Be(PcdReviewCoreType.EarlyAdvice);
        responseList[1].Type.Should().Be(PcdReviewCoreType.InitialReview);
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdReviewCore.Run"/> method returns an <see cref="UnprocessableEntityObjectResult"/> when the result is null.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_GetPcdReviewCoreReturnsNull_ReturnsUnprocessableEntity()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();

        this.communicationService.Setup(c => c.GetPcdReviewCoreAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<PcdReviewCoreResponseDto>)null!);

        // Act
        IActionResult result = await this.sut.Run(httpRequest, "54KR7689125", caseId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnprocessableEntityObjectResult>();
        Assert.Contains(this.mockLogger.Logs, log =>
         log.LogLevel == LogLevel.Error &&
         log.Message != null && log.Message.Contains("GetPcdReviewCore function failed"));
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdReviewCore.Run"/> method returns an <see cref="UnprocessableEntityObjectResult"/> when a BadRequestException is thrown.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsBadRequestException_ReturnsBadRequest()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();

        this.communicationService.Setup(c => c.GetPcdReviewCoreAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new BadRequestException("Invalid request", "caseId"));

        // Act
        IActionResult result = await this.sut.Run(httpRequest, "54KR7689125", caseId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which.Value.Should().Be("Invalid request (Parameter 'caseId')");
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdReviewCore.Run"/> method returns an <see cref="UnprocessableEntityObjectResult"/> when an InvalidOperationException is thrown.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsInvalidOperationException_ReturnsUnprocessableEntity()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();

        this.communicationService.Setup(c => c.GetPcdReviewCoreAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new InvalidOperationException("Test Exception"));

        // Act
        IActionResult result = await this.sut.Run(httpRequest, "54KR7689125", caseId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnprocessableEntityObjectResult>().Which.Value.Should().Be("Test Exception");
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdReviewCore.Run"/> method returns an <see cref="UnprocessableEntityObjectResult"/> when a NotSupportedException is thrown.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsNotSupportedException_ReturnsUnprocessableEntity()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();

        this.communicationService.Setup(c => c.GetPcdReviewCoreAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotSupportedException("Unsupported"));

        // Act
        IActionResult result = await this.sut.Run(httpRequest, "54KR7689125", caseId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnprocessableEntityObjectResult>().Which.Value.Should().Be("GetPcdReviewCore error: Unsupported");
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains("unsupported content type"));
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdReviewCore.Run"/> method returns an <see cref="UnauthorizedObjectResult"/> when an UnauthorizedAccessException is thrown.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsUnauthorizedAccessException_ReturnsUnauthorized()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();

        this.communicationService.Setup(c => c.GetPcdReviewCoreAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()))
           .ThrowsAsync(new UnauthorizedAccessException("Unauthorized"));

        // Act
        IActionResult result = await this.sut.Run(httpRequest, "54KR7689125", caseId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>().Which.Value.Should().Be("GetPcdReviewCore error: Unauthorized");
        Assert.Contains(this.mockLogger.Logs, log =>
                 log.LogLevel == LogLevel.Error &&
                 log.Message != null && log.Message.Contains("UnauthorizedAccess Exception"));
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdReviewCore.Run"/> method returns an <see cref="StatusCodeResult"/> with 500 when a general Exception is thrown.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_ThrowsGeneralException_ReturnsInternalServerError()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();

        this.communicationService.Setup(c => c.GetPcdReviewCoreAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()))
             .ThrowsAsync(new Exception("General error"));

        // Act
        IActionResult result = await this.sut.Run(httpRequest, "54KR7689125", caseId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(500);
        Assert.Contains(this.mockLogger.Logs, log =>
                    log.LogLevel == LogLevel.Error &&
                    log.Message != null && log.Message.Contains("encountered an error"));
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdReviewCore.Run"/> method returns a BadRequestObjectResult when caseId is invalid.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Run_InvalidCaseId_ReturnsBadRequest(int invalidCaseId)
    {
        // Arrange
        HttpRequest httpRequest = CreateHttpRequest();

        // Act
        IActionResult result = await this.sut.Run(httpRequest, "54KR7689125", invalidCaseId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which.Value.Should().Be($"{LoggingConstants.HskUiLogPrefix} Invalid case Id. It should be an integer.");
        this.communicationService.Verify(c => c.GetPcdReviewCoreAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdReviewCore.Run"/> method correctly passes the CancellationToken to the service.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_PassesCancellationToken_ToService()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var expectedResponse = new List<PcdReviewCoreResponseDto>();

        this.communicationService.Setup(c => c.GetPcdReviewCoreAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        await this.sut.Run(httpRequest, "54KR7689125", caseId, cancellationToken);

        // Assert
        this.communicationService.Verify(c => c.GetPcdReviewCoreAsync(caseId, It.IsAny<CmsAuthValues>(), cancellationToken), Times.Once);
    }

    /// <summary>
    /// Tests that the <see cref="GetPcdReviewCore.Run"/> method returns an empty collection when no PCD review items exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task Run_EmptyPcdReviewCore_ReturnsOkResultWithEmptyCollection()
    {
        // Arrange
        int caseId = 123;
        HttpRequest httpRequest = CreateHttpRequest();
        var expectedResponse = new List<PcdReviewCoreResponseDto>();

        this.communicationService.Setup(c => c.GetPcdReviewCoreAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        IActionResult result = await this.sut.Run(httpRequest, "54KR7689125", caseId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedResponse);
        var okResult = result as OkObjectResult;
        var response = okResult?.Value as IReadOnlyCollection<PcdReviewCoreResponseDto>;
        response.Should().NotBeNull();
        response!.Should().BeEmpty();
    }

    /// <summary>
    /// Helper method to create an HTTP request for testing.
    /// </summary>
    /// <returns>An <see cref="HttpRequest"/> object.</returns>
    private static HttpRequest CreateHttpRequest()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.HttpContext.Items["cookies"] = new Dictionary<string, string>
        {
            { "CMS-Token", "testToken" },
            { "User-Id", "testUser" },
            { "XSRF-TOKEN", "testXsrf" },
        };
        return request;
    }
}

