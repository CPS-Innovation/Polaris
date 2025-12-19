// <copyright file="BulkSetUnusedTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions.HouseKeeping;

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using Microsoft.Extensions.Logging;
using PolarisGateway.Functions.HouseKeeping;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Request;
using Common.Dto.Response.HouseKeeping;
using Common.Constants;
using System;

/// <summary>
/// Unit tests for the <see cref="BulkSetUnused"/> function.
/// </summary>
public class BulkSetUnusedTests
{
    private readonly TestLogger<BulkSetUnused> mockLogger;
    private readonly Mock<IBulkSetUnusedService> mockBulkSetUnusedService;
    private readonly BulkSetUnused bulkSetUnused;

    /// <summary>
    /// Initializes a new instance of the <see cref="BulkSetUnusedTests"/> class.
    /// Sets up mocks and the instance of <see cref="BulkSetUnused"/> for testing.
    /// </summary>
    public BulkSetUnusedTests()
    {
        this.mockLogger = new TestLogger<BulkSetUnused>();
        this.mockBulkSetUnusedService = new Mock<IBulkSetUnusedService>();
        this.bulkSetUnused = new BulkSetUnused(this.mockLogger, this.mockBulkSetUnusedService.Object);
    }

    /// <summary>
    /// Tests that a valid request returns an Ok result with a success status.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ValidRequest_ReturnsOk()
    {
        // Arrange
        var bulkSetUnusedRequests = new List<BulkSetUnusedRequest>
        {
            new BulkSetUnusedRequest { materialId = 1, subject = "Material 1" },
        };

        string requestBody = JsonSerializer.Serialize(bulkSetUnusedRequests);
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));
        httpRequest.Headers.Cookie = "valid_cookie";

        this.mockBulkSetUnusedService.Setup(service => service.BulkSetUnusedAsync(
            It.IsAny<int>(),
            It.IsAny<CmsAuthValues>(),
            It.IsAny<IReadOnlyCollection<BulkSetUnusedRequest>>()))
            .ReturnsAsync(new BulkSetUnusedResponse
            {
                Status = "success",
                ReclassifiedMaterials = new List<ReclassifiedMaterial>(),
                FailedMaterials = new List<FailedMaterial>(),
            });

        // Act
        IActionResult result = await this.bulkSetUnused.Run(httpRequest, 123);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal("success", ((BulkSetUnusedResponse)okResult.Value).Status);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} BulkSetUnused function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [123] BulkSetUnused function completed sucessfully"));
    }

    /// <summary>
    /// Tests that an empty request body returns a BadRequest result.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_EmptyRequestBody_ReturnsBadRequest()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Body = new MemoryStream(); // Empty body
        httpRequest.Headers.Cookie = "valid_cookie";

        // Act
        IActionResult result = await this.bulkSetUnused.Run(httpRequest, 123);

        // Assert
        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal($"{LoggingConstants.HskUiLogPrefix} Invalid or empty request body.", badRequestResult.Value);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} BulkSetUnused function processed a request."));
    }

    /// <summary>
    /// Tests that if the service returns a failed status, it returns an UnprocessableEntity result.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ServiceReturnsFailedStatus_ReturnsUnprocessableEntity()
    {
        // Arrange
        var bulkSetUnusedRequests = new List<BulkSetUnusedRequest>
        {
            new BulkSetUnusedRequest { materialId = 1, subject = "Material 1" },
        };

        string requestBody = JsonSerializer.Serialize(bulkSetUnusedRequests);
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));
        httpRequest.Headers.Cookie = "valid_cookie";

        this.mockBulkSetUnusedService.Setup(service => service.BulkSetUnusedAsync(
            It.IsAny<int>(),
            It.IsAny<CmsAuthValues>(),
            It.IsAny<IReadOnlyCollection<BulkSetUnusedRequest>>()))
            .ReturnsAsync(new BulkSetUnusedResponse
            {
                Status = "failed",
                ReclassifiedMaterials = new List<ReclassifiedMaterial>(),
                FailedMaterials = new List<FailedMaterial>(),
            });

        // Act
        IActionResult result = await this.bulkSetUnused.Run(httpRequest, 123);

        // Assert
        UnprocessableEntityObjectResult unprocessableEntityResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
        Assert.NotNull(unprocessableEntityResult.Value);
        Assert.Equal("failed", ((BulkSetUnusedResponse)unprocessableEntityResult.Value).Status);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} BulkSetUnused function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} caseId [123] BulkSetUnused function failed"));
    }

    /// <summary>
    /// Tests that if the service returns a partial success status, it returns a MultiStatus result.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ServiceReturnsPartialSuccess_ReturnsMultiStatus()
    {
        // Arrange
        var bulkSetUnusedRequests = new List<BulkSetUnusedRequest>
        {
            new BulkSetUnusedRequest { materialId = 1, subject = "Material 1" },
        };

        string requestBody = JsonSerializer.Serialize(bulkSetUnusedRequests);
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));
        httpRequest.Headers.Cookie = "valid_cookie";

        this.mockBulkSetUnusedService.Setup(service => service.BulkSetUnusedAsync(
            It.IsAny<int>(),
            It.IsAny<CmsAuthValues>(),
            It.IsAny<IReadOnlyCollection<BulkSetUnusedRequest>>()))
            .ReturnsAsync(new BulkSetUnusedResponse
            {
                Status = "partial_success",
                ReclassifiedMaterials = new List<ReclassifiedMaterial>(),
                FailedMaterials = new List<FailedMaterial>(),
            });

        // Act
        IActionResult result = await this.bulkSetUnused.Run(httpRequest, 123);

        // Assert
        ObjectResult multiStatusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status207MultiStatus, multiStatusResult.StatusCode);
        Assert.NotNull(multiStatusResult.Value);
        Assert.Equal("partial_success", ((BulkSetUnusedResponse)multiStatusResult.Value).Status);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} BulkSetUnused function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Warning &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [123] BulkSetUnused function completed partially successfully"));
    }

    /// <summary>
    /// Tests that if an exception is thrown, it returns an InternalServerError result.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var bulkSetUnusedRequests = new List<BulkSetUnusedRequest>
        {
            new BulkSetUnusedRequest { materialId = 1, subject = "Material 1" },
        };

        string requestBody = JsonSerializer.Serialize(bulkSetUnusedRequests);
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));
        httpRequest.Headers.Cookie = "valid_cookie";

        this.mockBulkSetUnusedService.Setup(service => service.BulkSetUnusedAsync(
            It.IsAny<int>(),
            It.IsAny<CmsAuthValues>(),
            It.IsAny<IReadOnlyCollection<BulkSetUnusedRequest>>()))
            .ThrowsAsync(new Exception($"Unexpected error"));

        // Act
        IActionResult result = await this.bulkSetUnused.Run(httpRequest, 123);

        // Assert
        StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} BulkSetUnused function encountered an error: Unexpected error"));
    }
}
