// <copyright file="GetWitnessStatementsTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions.HouseKeeping;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Constants;
using Common.Dto.Request;
using Common.Dto.Response.HouseKeeping;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Functions;
using PolarisGateway.Functions.HouseKeeping;
using Xunit;

/// <summary>
/// Unit tests for GetCaseWitnessStatements.
/// </summary>
public class GetCaseWitnessStatementsTests
{
    private readonly TestLogger<GetCaseWitnessStatements> mockLogger;
    private readonly Mock<IWitnessService> mockWitnessService;

    private readonly GetCaseWitnessStatements sutGetCaseWitnessStatements;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCaseWitnessStatementsTests"/> class.
    /// </summary>
    public GetCaseWitnessStatementsTests()
    {
        this.mockLogger = new TestLogger<GetCaseWitnessStatements>();
        this.mockWitnessService = new Mock<IWitnessService>();

        this.sutGetCaseWitnessStatements = new GetCaseWitnessStatements(
            this.mockLogger,
            this.mockWitnessService.Object);
    }

    /// <summary>
    /// Tests mwthod when witness Id is invalid or missling.
    /// </summary>
    /// <param name="witnessId">The witness ID.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Run_ReturnsBadRequest_WhenWitnessIdIsMissingOrInvalid(int witnessId)
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        // Act
        IActionResult result = await this.sutGetCaseWitnessStatements.Run(mockRequest.Object, 123, witnessId);

        // Assert
        Assert.Contains(this.mockLogger.Logs, log =>
           log.LogLevel == LogLevel.Information &&
           log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseWitnessStatements function processed a request."));

        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal($"{LoggingConstants.HskUiLogPrefix} Invalid witness_id format. It should be an integer.", badRequestResult.Value);
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

        var expectedResponse = new WitnessStatementsResponse();

        // Mock the witness service to return the expected response
        this.mockWitnessService
            .Setup(svc => svc.GetWitnessStatementsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(expectedResponse);

        // Act
        IActionResult result = await this.sutGetCaseWitnessStatements.Run(mockRequest.Object, 321, 121);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.IsType<WitnessStatementsResponse>(okResult.Value);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseWitnessStatements function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [321] witnessId [121] GetCaseWitnessStatements function completed"));

        this.mockWitnessService.Verify(
            svc => svc.GetWitnessStatementsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()), Times.Once);
    }

    /// <summary>
    /// Tests that a valid request returns an Ok result with a success status.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityError_WhenInvalidOperationExceptionIsThrown()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        var expectedResponse = new UsedStatementsResponse();

        // Mock the witness service to return the expected response
        this.mockWitnessService
            .Setup(svc => svc.GetWitnessStatementsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new InvalidOperationException("Invalid operation error."));

        // Act
        IActionResult result = await this.sutGetCaseWitnessStatements.Run(mockRequest.Object, 322, 121);

        // Assert
        ObjectResult objectResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseWitnessStatements function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
              log.LogLevel == LogLevel.Error &&
              log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseWitnessStatements function encountered an invalid operation error: Invalid operation error"));

    }

    /// <summary>
    /// Tests that a valid request returns an Ok result with a success status.
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
            .Setup(svc => svc.GetWitnessStatementsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new UnauthorizedAccessException("Unauthorized"));

        // Act
        IActionResult result = await this.sutGetCaseWitnessStatements.Run(mockRequest.Object, 232, 121);

        // Assert
        UnauthorizedObjectResult unauthorizedAccessResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedAccessResult.StatusCode);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseWitnessStatements function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
             log.LogLevel == LogLevel.Error &&
             log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseWitnessStatements function encountered an unauthorized access error: Unauthorized"));
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
