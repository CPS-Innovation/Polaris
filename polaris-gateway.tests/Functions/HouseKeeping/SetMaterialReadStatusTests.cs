// <copyright file="SetMaterialReadStatusTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions.HouseKeeping;

using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Constants;
using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.HouseKeeping;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Functions.HouseKeeping;
using Xunit;

/// <summary>
/// Unit tests for the <see cref="sut"/> function.
/// </summary>
public class SetMaterialReadStatusTests
{
    private readonly TestLogger<SetMaterialReadStatus> mockLogger;
    private readonly Mock<ICommunicationService> mockCommunicationService;
    private readonly SetMaterialReadStatus sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetMaterialReadStatusTests"/> class.
    /// Sets up mocks and the instance of <see cref="sut"/> for testing.
    /// </summary>
    public SetMaterialReadStatusTests()
    {
        this.mockLogger = new TestLogger<SetMaterialReadStatus>();
        this.mockCommunicationService = new Mock<ICommunicationService>();
        this.sut = new SetMaterialReadStatus(
            this.mockLogger,
            this.mockCommunicationService.Object);
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

        var requestBody = new SetMaterialReadStatusRequest(Guid.NewGuid(), 1212, Common.Enums.MaterialReadStatusType.Read);
        string httpRequestString = JsonSerializer.Serialize(requestBody);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var expectedResponse = new SetMaterialReadStatusResponse(new SetMaterialReadStatusResponseData
        {
            Id = 1212,
        });

        // Mock the communication service to return an empty collection for communications
        this.mockCommunicationService
            .Setup(service => service.SetMaterialReadStatusAsync(It.IsAny<int>(), It.IsAny<Common.Enums.MaterialReadStatusType>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ReturnsAsync(expectedResponse);

        // Act
        IActionResult result = await this.sut.Run(httpRequest, 123);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(1212, ((SetMaterialReadStatusResponse)okResult.Value)?.CompleteCommunicationData?.Id);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [123] and Material id [1212] SetMaterialReadStatus function completed"));
    }

    /// <summary>
    /// Tests that if the service returns a failed status, it returns an StattusCode result.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ServiceReturnsFailedStatus_ReturnsStatusCodeResultAsExpected()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";

        var requestBody = new SetMaterialReadStatusRequest(Guid.NewGuid(), 1212, Common.Enums.MaterialReadStatusType.Read);
        string httpRequestString = JsonSerializer.Serialize(requestBody);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var expectedResponse = new SetMaterialReadStatusResponse(new SetMaterialReadStatusResponseData
        {
        });

        // Mock the communication service to return an empty collection for communications
        this.mockCommunicationService
             .Setup(service => service.SetMaterialReadStatusAsync(It.IsAny<int>(), It.IsAny<Common.Enums.MaterialReadStatusType>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .Throws(new JsonException("Unable to parse object"));

        // Act
        IActionResult result = await this.sut.Run(httpRequest, 123);

        // Assert
        StatusCodeResult unprocessableEntityResult = Assert.IsType<StatusCodeResult>(result);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} SetMaterialReadStatus function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} SetMaterialReadStatus function encountered an error"));
    }

    /// <summary>
    /// Tests that an invalid cookie returns a BadRequest result.
    /// </summary>
    /// <param name="materialId">Material id.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [InlineData(0)]
    [Theory]
    public async Task Run_InvalidRequestData_ReturnsBadRequest(int materialId)
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";

        var requestBody = new SetMaterialReadStatusRequest(Guid.NewGuid(), materialId, Common.Enums.MaterialReadStatusType.Invalid);
        string httpRequestString = JsonSerializer.Serialize(requestBody);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var expectedResponse = new SetMaterialReadStatusResponse(new SetMaterialReadStatusResponseData
        {
            Id = 1212,
        });

        // Mock the communication service to return an empty collection for communications
        this.mockCommunicationService
              .Setup(service => service.SetMaterialReadStatusAsync(It.IsAny<int>(), It.IsAny<Common.Enums.MaterialReadStatusType>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ReturnsAsync(expectedResponse);

        // Act
        IActionResult result = await this.sut.Run(httpRequest, 123);

        // Assert
        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} SetMaterialReadStatus function processed a request."));
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

        var requestBody = new SetMaterialReadStatusRequest(Guid.NewGuid(), 1212, Common.Enums.MaterialReadStatusType.Read);
        string httpRequestString = JsonSerializer.Serialize(requestBody);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var expectedResponse = new SetMaterialReadStatusResponse(new SetMaterialReadStatusResponseData
        {
        });

        var mockedException = new InvalidOperationException("Invalid json format");

        // Mock the communication service to return an empty collection for communications
        this.mockCommunicationService
            .Setup(service => service.SetMaterialReadStatusAsync(It.IsAny<int>(), It.IsAny<Common.Enums.MaterialReadStatusType>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
           .Throws(mockedException);

        // Act
        IActionResult result = await this.sut.Run(httpRequest, 123);

        // Assert
        UnprocessableEntityObjectResult unprocessableEntityResult = Assert.IsType<UnprocessableEntityObjectResult>(result);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} SetMaterialReadStatus function processed a request."));

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

        var requestBody = new SetMaterialReadStatusRequest(Guid.NewGuid(), 1212, Common.Enums.MaterialReadStatusType.Read);
        string httpRequestString = JsonSerializer.Serialize(requestBody);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var expectedResponse = new SetMaterialReadStatusResponse(new SetMaterialReadStatusResponseData
        {
        });

        var mockedException = new NotSupportedException("Invalid json format");

        // Mock the communication service to return an empty collection for communications
        this.mockCommunicationService
            .Setup(service => service.SetMaterialReadStatusAsync(It.IsAny<int>(), It.IsAny<Common.Enums.MaterialReadStatusType>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .Throws(mockedException);

        // Act
        IActionResult result = await this.sut.Run(httpRequest, 123);

        // Assert
        UnprocessableEntityObjectResult unprocessableEntityResult = Assert.IsType<UnprocessableEntityObjectResult>(result);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} SetMaterialReadStatus function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} SetMaterialReadStatus function encountered unsupported content type"));
    }
}
