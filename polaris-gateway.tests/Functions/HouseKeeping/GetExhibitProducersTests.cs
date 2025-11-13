// <copyright file="GetExhibitProducersTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions.HouseKeeping;

using System;
using System.Collections.Generic;
using System.Linq;
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
using PolarisGateway.Functions.HouseKeeping;
using Xunit;

/// <summary>
/// Unit tests for GetExhibitProducers function.
/// </summary>
public class GetExhibitProducersTests
{
    private readonly TestLogger<GetCaseExhibitProducers> mockLogger;
    private readonly Mock<ICommunicationService> mockCommunicationService;
    private readonly Mock<IWitnessService> mockWitnessService;
    private readonly GetCaseExhibitProducers sutGetExhibitProducers;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetExhibitProducersTests"/> class.
    /// </summary>
    public GetExhibitProducersTests()
    {
        mockLogger = new TestLogger<GetCaseExhibitProducers>();
        mockCommunicationService = new Mock<ICommunicationService>();

        mockWitnessService = new Mock<IWitnessService>();

        sutGetExhibitProducers = new GetCaseExhibitProducers(
            mockLogger,
            mockCommunicationService.Object,
            mockWitnessService.Object);
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
        var expectedProducers = new ExhibitProducersResponse
        {
            ExhibitProducers = new List<ExhibitProducer>()
            {
                new (Id: 343, "Joe SMITH", false),
                new (Id: 346, "Bob JACKSON", false),
            },
        };

        var witnesses = new WitnessesResponse
        {
            Witnesses = new List<Witness>()
            {
                new Witness(CaseId: 221, WitnessId: 34, "Jane", "Jones"),
                new Witness(CaseId: 221, WitnessId: 36, "Bill", "Ted"),
            },
        };

        mockCommunicationService
            .Setup(x => x.GetExhibitProducersAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(expectedProducers);

        mockWitnessService
            .Setup(x => x.GetCaseWitnessesAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(witnesses);

        // Act
        IActionResult result = await this.sutGetExhibitProducers.Run(mockRequest.Object, 321);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.IsType<ExhibitProducersResponse>(okResult.Value);

        var producers = (ExhibitProducersResponse)okResult.Value;

        Assert.NotNull(producers);
        Assert.Equal(4, producers.ExhibitProducers!.Count);
        Assert.Equal(2, producers.ExhibitProducers.Where(x => x.IsWitness).Count());
        Assert.Equal("Jane Jones", producers.ExhibitProducers.Where(x => x.IsWitness).First().Name);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseExhibitProducers function processed a request."));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [321] GetCaseExhibitProducers function completed"));

        mockCommunicationService.Verify(
            svc => svc.GetExhibitProducersAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()), Times.Once);
    }

    /// <summary>
    /// Tests that a valid request returns an Ok result with a success status.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ReturnsOkResult_WhenCaseHasExhibitsButNoWitnesses_ReturnsExhibitsOnly()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        var expectedProducers = new ExhibitProducersResponse
        {
            ExhibitProducers = new List<ExhibitProducer>()
            {
                new (Id: 343, "Joe SMITH", false),
                new (Id: 346, "Bob JACKSON", false),
            },
        };

        var witnesses = new WitnessesResponse
        {
            Witnesses = null,
        };

        mockCommunicationService
            .Setup(x => x.GetExhibitProducersAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(expectedProducers);

        mockWitnessService
            .Setup(x => x.GetCaseWitnessesAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(witnesses);

        // Act
        IActionResult result = await sutGetExhibitProducers.Run(mockRequest.Object, 321);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.IsType<ExhibitProducersResponse>(okResult.Value);

        var producers = (ExhibitProducersResponse)okResult.Value;

        Assert.NotNull(producers);
        Assert.Equal(2, producers.ExhibitProducers!.Count);
        Assert.Equal(2, producers.ExhibitProducers.Where(x => x.IsWitness == false).Count());

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseExhibitProducers function processed a request."));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [321] GetCaseExhibitProducers function completed"));

        mockCommunicationService.Verify(
            svc => svc.GetExhibitProducersAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()), Times.Once);
    }

    /// <summary>
    /// Tests that a valid request returns an Ok result with a success status.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ReturnsOkResult_WhenCaseHasWitnessesButNoExhibits()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        var expectedProducers = new ExhibitProducersResponse
        {
            ExhibitProducers = null,
        };

        var witnesses = new WitnessesResponse
        {
            Witnesses = new List<Witness>()
            {
                new Witness(CaseId: 221, WitnessId: 34, "Jane", "Jones"),
                new Witness(CaseId: 221, WitnessId: 36, "Bill", "Ted"),
            },
        };

        mockCommunicationService
            .Setup(x => x.GetExhibitProducersAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(expectedProducers);

        mockWitnessService
            .Setup(x => x.GetCaseWitnessesAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(witnesses);

        // Act
        IActionResult result = await sutGetExhibitProducers.Run(mockRequest.Object, 321);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.IsType<ExhibitProducersResponse>(okResult.Value);

        var producers = (ExhibitProducersResponse)okResult.Value;

        Assert.NotNull(producers);
        Assert.Equal(2, producers.ExhibitProducers!.Count);
        Assert.Equal(2, producers.ExhibitProducers.Where(x => x.IsWitness == true).Count());

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseExhibitProducers function processed a request."));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [321] GetCaseExhibitProducers function completed"));

        mockCommunicationService.Verify(
            svc => svc.GetExhibitProducersAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()), Times.Once);
    }

    /// <summary>
    /// Tests that when an invalid operation exception is thrown, it is handled and logged as error.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityError_WhenInvalidOperationExceptionIsThrown()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        var expectedProducers = new ExhibitProducersResponse
        {
            ExhibitProducers = new List<ExhibitProducer>()
            {
                new (Id: 343, "Joe SMITH", false),
                new (Id: 343, "Bob JACKSON", false),
            },
        };

        mockCommunicationService
            .Setup(x => x.GetExhibitProducersAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new InvalidOperationException("Invalid operation error"));

        // Act
        IActionResult result = await sutGetExhibitProducers.Run(mockRequest.Object, 321);

        // Assert
        ObjectResult objectResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseExhibitProducers function processed a request."));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseExhibitProducers function encountered an invalid operation error: Invalid operation error"));

        mockCommunicationService.Verify(
            svc => svc.GetExhibitProducersAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()), Times.Once);
    }

    /// <summary>
    /// Tests that when UnauthorizedAccessException exception is thrown by the API, it is handled and logged as error.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityError_WhenUnauthorizedAccessExceptionIsThrown()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        var expectedProducers = new ExhibitProducersResponse
        {
            ExhibitProducers = new List<ExhibitProducer>()
            {
                new (Id: 343, "Joe SMITH", false),
                new (Id: 343, "Bob JACKSON", false),
            },
        };

        mockCommunicationService
          .Setup(x => x.GetExhibitProducersAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
          .ThrowsAsync(new UnauthorizedAccessException("Unauthorized"));

        // Act
        IActionResult result = await sutGetExhibitProducers.Run(mockRequest.Object, 321);

        // Assert
        UnauthorizedObjectResult unauthorizedAccessResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedAccessResult.StatusCode);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseExhibitProducers function processed a request."));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseExhibitProducers function encountered an unauthorized access error: Unauthorized"));

        mockCommunicationService.Verify(
            svc => svc.GetExhibitProducersAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()), Times.Once);
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
