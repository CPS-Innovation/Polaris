// <copyright file="RenameMaterialTests.cs" company="TheCrownProsecutionService">
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
using Cps.Fct.Hk.Ui.Services.Validators;
using PolarisGateway.Functions.HouseKeeping;
using Common.Dto.Request.HouseKeeping;
using System.IO;
using Common.Dto.Request;
using Common.Constants;
using System;
using Common.Dto.Response.HouseKeeping;

/// <summary>
/// Unit tests for the <see cref="RenameMaterial"/> function.
/// </summary>
public class RenameMaterialTests
{
    private readonly TestLogger<RenameMaterial> mockLogger;
    private readonly Mock<ICommunicationService> mockCommunicationService;
    private readonly RenameMaterial renameMaterial;
    private readonly RenameMaterialRequestValidator validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="RenameMaterialTests"/> class.
    /// Sets up mocks and the instance of <see cref="RenameMaterial"/> for testing.
    /// </summary>
    public RenameMaterialTests()
    {
        this.validator = new RenameMaterialRequestValidator();
        this.mockLogger = new TestLogger<RenameMaterial>();
        this.mockCommunicationService = new Mock<ICommunicationService>();
        
        this.renameMaterial = new RenameMaterial(
            this.mockLogger,
            this.mockCommunicationService.Object,
            this.validator);
    }

    /// <summary>
    /// Tests that a valid request returns an Ok result with a success status.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ValidRequest_ReturnsOk()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";

        var requestBody = new RenameMaterialRequest(Guid.NewGuid(), 1212, "test name");
        string httpRequestString = JsonSerializer.Serialize(requestBody);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var expectedResponse = new RenameMaterialResponse(new RenameMaterialData
        {
            Id = 1212,
        });

        // Mock the communication service to return an empty collection for communications
        this.mockCommunicationService
            .Setup(service => service.RenameMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ReturnsAsync(expectedResponse);

        // Act
        IActionResult result = await this.renameMaterial.Run(httpRequest, 123, 456);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(1212, ((RenameMaterialResponse)okResult.Value)?.RenameMaterialData?.Id);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [123] RenameMaterial function completed"));
    }

    /// <summary>
    /// Tests that if the service returns a failed status, it returns an StatusCode result.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ServiceReturnsFailedStatus_ReturnsStatusCodeResultAsExpected()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";

        var requestBody = new RenameMaterialRequest(Guid.NewGuid(), 1212, "test name");
        string httpRequestString = JsonSerializer.Serialize(requestBody);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var expectedResponse = new RenameMaterialResponse(new RenameMaterialData
        {
        });

        // Mock the communication service to return an empty collection for communications
        this.mockCommunicationService
            .Setup(service => service.RenameMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .Throws(new JsonException("Unable to parse object"));

        // Act
        IActionResult result = await this.renameMaterial.Run(httpRequest, 123, 345);

        // Assert
        StatusCodeResult unprocessableEntityResult = Assert.IsType<StatusCodeResult>(result);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} RenameMaterial function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} RenameMaterial function encountered an error"));
    }

    /// <summary>
    /// Tests that an invalid cookie returns a BadRequest result.
    /// </summary>
    /// <param name="materialId">Material id.</param>
    /// <param name="subject">Material name being tested.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [InlineData(0, "test")]
    [InlineData(1212, "")]
#pragma warning disable xUnit1012 // Null should only be used for nullable parameters
    [InlineData(1212, null)]
#pragma warning restore xUnit1012 // Null should only be used for nullable parameters
    [InlineData(1212, " ")]
    [Theory]
    public async Task Run_InvalidRequestData_ReturnsBadRequest(int materialId, string subject)
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";

        var requestBody = new RenameMaterialRequest(Guid.NewGuid(), materialId, subject);
        string httpRequestString = JsonSerializer.Serialize(requestBody);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var expectedResponse = new RenameMaterialResponse(new RenameMaterialData
        {
            Id = 1212,
        });

        // Mock the communication service to return an empty collection for communications
        this.mockCommunicationService
              .Setup(service => service.RenameMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ReturnsAsync(expectedResponse);

        // Act
        IActionResult result = await this.renameMaterial.Run(httpRequest, 122, 123);

        // Assert
        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} RenameMaterial function processed a request."));
    }

    /// <summary>
    /// Tests that if the service returns a failed status, it returns an UnprocessableEntity result.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";

        var requestBody = new RenameMaterialRequest(Guid.NewGuid(), 1212, "test name");
        string httpRequestString = JsonSerializer.Serialize(requestBody);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var expectedResponse = new RenameMaterialResponse(new RenameMaterialData
        {
        });

        var mockedException = new InvalidOperationException("Invalid json format");

        // Mock the communication service to return an empty collection for communications
        this.mockCommunicationService
            .Setup(service => service.RenameMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
           .Throws(mockedException);

        // Act
        IActionResult result = await this.renameMaterial.Run(httpRequest, 122, 233);

        // Assert
        UnprocessableEntityObjectResult unprocessableEntityResult = Assert.IsType<UnprocessableEntityObjectResult>(result);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} RenameMaterial function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains("Invalid json format"));
    }

    /// <summary>
    /// Tests that if the service returns a failed status, it returns an UnprocessableEntity result.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_NotSupportedExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";

        var requestBody = new RenameMaterialRequest(Guid.NewGuid(), 1212, "test name");
        string httpRequestString = JsonSerializer.Serialize(requestBody);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var expectedResponse = new RenameMaterialResponse(new RenameMaterialData
        {
        });

        var mockedException = new NotSupportedException("Invalid json format");

        // Mock the communication service to return an empty collection for communications
        this.mockCommunicationService
            .Setup(service => service.RenameMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .Throws(mockedException);

        // Act
        IActionResult result = await this.renameMaterial.Run(httpRequest, 323, 232);

        // Assert
        UnprocessableEntityObjectResult unprocessableEntityResult = Assert.IsType<UnprocessableEntityObjectResult>(result);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} RenameMaterial function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} RenameMaterial function encountered unsupported content type"));
    }
}
