// <copyright file="DiscardMaterialTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions.HouseKeeping;

using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;
using Cps.Fct.Hk.Ui.Functions.Functions;
using PolarisGateway.Functions.HouseKeeping;
using Common.Dto.Request.HouseKeeping;
using System;
using System.IO;
using Common.Dto.Response.HouseKeeping;
using Common.Dto.Request;
using Common.Constants;

/// <summary>
/// Unit tests for the <see cref="DiscardMaterial"/> function.
/// </summary>
public class DiscardMaterialTests
{
    private readonly TestLogger<DiscardMaterial> mockLogger;
    private readonly Mock<ICommunicationService> mockCommunicationService;
    private readonly DiscardMaterial discardMaterial;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiscardMaterialTests"/> class.
    /// Sets up mocks and the instance of <see cref="DiscardMaterial"/> for testing.
    /// </summary>
    public DiscardMaterialTests()
    {
        this.mockLogger = new TestLogger<DiscardMaterial>();
        this.mockCommunicationService = new Mock<ICommunicationService>();
        
        this.discardMaterial = new DiscardMaterial(
            this.mockLogger,
            this.mockCommunicationService.Object);
    }

    /// <summary>
    /// Tests that a valid request returns an Ok result with a success status.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ReturnsOkResult_WhenValidRequestProvided()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";

        var requestBody = new DiscardMaterialRequest(Guid.NewGuid(), 1212, "discard reason", "discard reason description");
        string httpRequestString = JsonSerializer.Serialize(requestBody);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var expectedResponse = new DiscardMaterialResponse(new DiscardMaterialData
        {
            Id = 1212,
        });

        // Mock the communication service to return the expected response
        this.mockCommunicationService
            .Setup(service => service.DiscardMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ReturnsAsync(expectedResponse);

        // Act
        IActionResult result = await this.discardMaterial.Run(httpRequest, 123, 456);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(1212, ((DiscardMaterialResponse)okResult.Value)?.DiscardMaterialData?.Id);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} DiscardMaterial function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [123] DiscardMaterial function completed"));
    }

    /// <summary>
    /// Tests that the function returns an unprocessable entity error when an invalid operation exception is thrown.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityError_WhenInvalidOperationExceptionIsThrown()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        this.mockCommunicationService
            .Setup(service => service.DiscardMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ThrowsAsync(new InvalidOperationException("Invalid operation error"));

        var requestBody = new DiscardMaterialRequest(Guid.NewGuid(), 1212, "discard reason", "discard reason description");
        string httpRequestString = JsonSerializer.Serialize(requestBody);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        mockRequest.Setup(x => x.Body).Returns(stream);

        var expectedResponse = new DiscardMaterialResponse(new DiscardMaterialData
        {
            Id = 1212,
        });

        // Act
        IActionResult result = await this.discardMaterial.Run(mockRequest.Object, 123, 456);

        // Assert
        ObjectResult objectResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} DiscardMaterial function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} DiscardMaterial function encountered an invalid operation error: Invalid operation error"));
    }

    /// <summary>
    /// Tests that the function returns an unprocessable entity error when a not supported exception is thrown.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityError_WhenNotSupportedExceptionIsThrown()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        this.mockCommunicationService
            .Setup(service => service.DiscardMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ThrowsAsync(new NotSupportedException("Not supported error"));

        var requestBody = new DiscardMaterialRequest(Guid.NewGuid(), 1212, "discard reason", "discard reason description");
        string httpRequestString = JsonSerializer.Serialize(requestBody);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        mockRequest.Setup(x => x.Body).Returns(stream);

        var expectedResponse = new DiscardMaterialResponse(new DiscardMaterialData
        {
            Id = 1212,
        });

        // Act
        IActionResult result = await this.discardMaterial.Run(mockRequest.Object, 1212, 456);

        // Assert
        ObjectResult objectResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} DiscardMaterial function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} DiscardMaterial function encountered an unsupported content type error: Not supported error"));
    }

    /// <summary>
    /// Tests that the function returns an unauthorized error when unauthorized exception is thrown.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnauthorizedError_WhenUnauthorizedAccessExceptionIsThrown()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        this.mockCommunicationService
            .Setup(service => service.DiscardMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ThrowsAsync(new UnauthorizedAccessException("Unauthorized"));

        var requestBody = new DiscardMaterialRequest(Guid.NewGuid(), 1212, "discard reason", "discard reason description");
        string httpRequestString = JsonSerializer.Serialize(requestBody);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        mockRequest.Setup(x => x.Body).Returns(stream);

        var expectedResponse = new DiscardMaterialResponse(new DiscardMaterialData
        {
            Id = 1212,
        });

        // Act
        IActionResult result = await this.discardMaterial.Run(mockRequest.Object, 123, 1212);

        // Assert
        UnauthorizedObjectResult unauthorizedAccessResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedAccessResult.StatusCode);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} DiscardMaterial function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} DiscardMaterial function encountered an unauthorized access error: Unauthorized"));
    }

    /// <summary>
    /// Tests that if the service returns a failed status, it returns an StatusCode result.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ReturnsInternalServerError_WhenExceptionIsThrown()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";

        var requestBody = new DiscardMaterialRequest(Guid.NewGuid(), 1212, "discard reason", "discard reason description");
        string httpRequestString = JsonSerializer.Serialize(requestBody);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var expectedResponse = new DiscardMaterialResponse(new DiscardMaterialData
        {
        });

        // Mock the communication service to throw an exception
        this.mockCommunicationService
            .Setup(service => service.DiscardMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .Throws(new Exception("Simulated service exception"));

        // Act
        IActionResult result = await this.discardMaterial.Run(httpRequest, 1212, 456);

        // Assert
        StatusCodeResult unprocessableEntityResult = Assert.IsType<StatusCodeResult>(result);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} DiscardMaterial function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} DiscardMaterial function encountered an error: Simulated service exception"));
    }

    private static Mock<HttpRequest> SetUpMockRequest()
    {
        var mockRequest = new Mock<HttpRequest>();

        // Set up a DefaultHttpContext to support setting headers
        var context = new DefaultHttpContext();
        mockRequest.Setup(r => r.HttpContext).Returns(context);
        mockRequest.Setup(r => r.Headers.Add("corelation", "1232131231"));

        return mockRequest;
    }
}
