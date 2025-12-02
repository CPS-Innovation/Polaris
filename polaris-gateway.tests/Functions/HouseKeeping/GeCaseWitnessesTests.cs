// <copyright file="GeCaseWitnesses.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions.HouseKeeping;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Constants;
using Common.Dto.Request;
using Common.Dto.Response.HouseKeeping;
using Cps.Fct.Hk.Ui.Functions.Functions;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for GetCaseWitnesses.
/// </summary>
public class GeCaseWitnessesTests
{
    private readonly TestLogger<GetCaseWitnesses> mockLogger;
    private readonly Mock<IWitnessService> mockWitnessService;

    private readonly GetCaseWitnesses sutGetCaseWitnesses;

    /// <summary>
    /// Initializes a new instance of the <see cref="GeCaseWitnessesTests"/> class.
    /// </summary>
    public GeCaseWitnessesTests()
    {
        this.mockLogger = new TestLogger<GetCaseWitnesses>();
        this.mockWitnessService = new Mock<IWitnessService>();

        this.sutGetCaseWitnesses = new GetCaseWitnesses(
            this.mockLogger,
            this.mockWitnessService.Object);
    }

    /// <summary>
    /// Tests that a valid request returns an Ok result with a success status.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ReturnsOkResult_WhenValidRequestProvidedAndServiceCallIsSuccessful()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        var expectedWitnesses = new WitnessesResponse
        {
            Witnesses = new List<Witness>()
            {
                new (123, 343, "Joe", "SMITH"),
                new (123, 432, "Bob", "Jackson"),
            },
        };

        // Mock the witness service to return the expected response
        this.mockWitnessService
            .Setup(svc => svc.GetCaseWitnessesAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(expectedWitnesses);

        // Act
        IActionResult result = await this.sutGetCaseWitnesses.Run(mockRequest.Object, 321);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.IsType<WitnessesResponse>(okResult.Value);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseWitnesses function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [321] GetCaseWitnesses function completed"));

        this.mockWitnessService.Verify(
            svc => svc.GetCaseWitnessesAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()), Times.Once);
    }

    /// <summary>
    /// Tests that when an invalid operation exception is thrown, it is logged as error.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityError_WhenInvalidOperationExceptionIsThrown()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        var expectedWitnesses = new WitnessesResponse
        {
            Witnesses = new List<Witness>()
            {
                new (123, 343, "Joe", "SMITH"),
                new (123, 432, "Bob", "Jackson"),
            },
        };

        // Mock the witness service to return the expected response
        this.mockWitnessService
            .Setup(svc => svc.GetCaseWitnessesAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new InvalidOperationException("Invalid operation error."));

        // Act
        IActionResult result = await this.sutGetCaseWitnesses.Run(mockRequest.Object, 321);

        // Assert
        ObjectResult objectResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseWitnesses function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
              log.LogLevel == LogLevel.Error &&
              log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseWitnesses function encountered an invalid operation error: Invalid operation error"));

        this.mockWitnessService.Verify(
            svc => svc.GetCaseWitnessesAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()), Times.Once);
    }

    /// <summary>
    /// Tests that when UnauthorizedAccessException exception is thrown by the API, it is handled and logged as error.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnauthorizedError_WhenUnauthorizedAccessExceptionIsThrown()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        var expectedWitnesses = new WitnessesResponse
        {
            Witnesses = new List<Witness>()
            {
                new (123, 343, "Joe", "SMITH"),
                new (123, 432, "Bob", "Jackson"),
            },
        };

        // Mock the witness service to return the expected response
        this.mockWitnessService
            .Setup(svc => svc.GetCaseWitnessesAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new UnauthorizedAccessException("Unauthorized"));

        // Act
        IActionResult result = await this.sutGetCaseWitnesses.Run(mockRequest.Object, 321);

        // Assert
        UnauthorizedObjectResult unauthorizedAccessResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedAccessResult.StatusCode);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseWitnesses function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
             log.LogLevel == LogLevel.Error &&
             log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseWitnesses function encountered an unauthorized access error: Unauthorized"));

        this.mockWitnessService.Verify(
            svc => svc.GetCaseWitnessesAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()), Times.Once);
    }

    private static Mock<HttpRequest> SetUpMockRequest()
    {
        var mockRequest = new Mock<HttpRequest>();

        // Set up a DefaultHttpContext to support setting headers
        var context = new DefaultHttpContext();
        mockRequest.Setup(r => r.HttpContext).Returns(context);
        mockRequest.Setup(r => r.Headers.Add("corelation", "1232131231"));
        mockRequest.Setup(c => c.Cookies["token"]);

        return mockRequest;
    }
}
