// <copyright file="UmaReclassifyTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions.HouseKeeping;

using System.Collections.Generic;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Interfaces.Model;
using Cps.Fct.Hk.Ui.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using Microsoft.Extensions.Logging;
using PolarisGateway.Functions.HouseKeeping;
using Common.Dto.Response.HouseKeeping;
using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Constants;
using System;

/// <summary>
/// Unit tests for the <see cref="UmaReclassify"/> function.
/// </summary>
public class UmaReclassifyTests
{
    private readonly TestLogger<UmaReclassify> mockLogger;
    private readonly Mock<ICommunicationService> mockCommunicationService;
    private readonly Mock<IUmaReclassifyService> mockUmaReclassifyService;
    private readonly Mock<IBulkSetUnusedService> mockBulkSetUnusedService;
    private readonly UmaReclassify umaReclassify;

    /// <summary>
    /// Initializes a new instance of the <see cref="UmaReclassifyTests"/> class.
    /// Sets up mocks and the instance of <see cref="UmaReclassify"/> for testing.
    /// </summary>
    public UmaReclassifyTests()
    {
        this.mockLogger = new TestLogger<UmaReclassify>();
        this.mockCommunicationService = new Mock<ICommunicationService>();
        this.mockUmaReclassifyService = new Mock<IUmaReclassifyService>();
        this.mockBulkSetUnusedService = new Mock<IBulkSetUnusedService>();
        this.umaReclassify = new UmaReclassify(
            this.mockLogger,
            this.mockCommunicationService.Object,
            this.mockUmaReclassifyService.Object,
            this.mockBulkSetUnusedService.Object);
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

        var communications = new List<Communication>
        {
            new Communication(
                Id: 1,
                OriginalFileName: "file1.pdf",
                Subject: "Subject 1",
                DocumentTypeId: 1,
                MaterialId: 1001,
                Link: "http://example.com/file1",
                Status: "Pending",
                Category: "Category 1",
                Type: "Type A",
                HasAttachments: false),

            new Communication(
                Id: 2,
                OriginalFileName: "file2.pdf",
                Subject: "Subject 2",
                DocumentTypeId: 2,
                MaterialId: 1002,
                Link: "http://example.com/file2",
                Status: "Processed",
                Category: "Category 2",
                Type: "Type B",
                HasAttachments: false),
        };

        var matchedCommunications = new List<MatchedCommunication>
        {
            new MatchedCommunication { materialId = 2, subject = "Subject 2" },
        };

  
        // Mock the communication service to return an empty collection for communications
        this.mockCommunicationService
            .Setup(service => service.GetCommunicationsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(communications);

        // Mock the service to return matched communications
        this.mockUmaReclassifyService
            .Setup(service => service.ProcessMatchingRequest(It.IsAny<int>(), It.IsAny<IReadOnlyCollection<Communication>>()))
            .ReturnsAsync(matchedCommunications);

        // Mock the bulk set unused service to return a success status
        this.mockBulkSetUnusedService
            .Setup(service => service.BulkSetUnusedAsync(
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
        IActionResult result = await this.umaReclassify.Run(httpRequest, 123);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal("success", ((BulkSetUnusedResponse)okResult.Value).Status);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} UmaReclassify function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Retrieving communications for caseId [123]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [123] UmaReclassify/BulkSetUnused function completed"));
    }

    /// <summary>
    /// Tests that if the service returns a failed status, it returns an UnprocessableEntity result.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ServiceReturnsFailedStatus_ReturnsUnprocessableEntity()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";

        var communications = new List<Communication>
        {
            new Communication(
                Id: 1,
                OriginalFileName: "file1.pdf",
                Subject: "Subject 1",
                DocumentTypeId: 1,
                MaterialId: 1001,
                Link: "http://example.com/file1",
                Status: "Pending",
                Category: "Category 1",
                Type: "Type A",
                HasAttachments: false),

            new Communication(
                Id: 2,
                OriginalFileName: "file2.pdf",
                Subject: "Subject 2",
                DocumentTypeId: 2,
                MaterialId: 1002,
                Link: "http://example.com/file2",
                Status: "Processed",
                Category: "Category 2",
                Type: "Type B",
                HasAttachments: false),
        };

        var matchedCommunications = new List<MatchedCommunication>
        {
            new MatchedCommunication { materialId = 2, subject = "Subject 2" },
        };

        // Mock the communication service to return an empty collection for communications
        this.mockCommunicationService
            .Setup(service => service.GetCommunicationsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(communications);

        // Mock the service to return matched communications
        this.mockUmaReclassifyService
            .Setup(service => service.ProcessMatchingRequest(It.IsAny<int>(), It.IsAny<IReadOnlyCollection<Communication>>()))
            .ReturnsAsync(matchedCommunications);

        // Mock the bulk set unused service to return a failed status
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
        IActionResult result = await this.umaReclassify.Run(httpRequest, 123);

        // Assert
        UnprocessableEntityObjectResult unprocessableEntityResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
        Assert.NotNull(unprocessableEntityResult.Value);
        Assert.Equal("failed", ((BulkSetUnusedResponse)unprocessableEntityResult.Value).Status);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} UmaReclassify function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Retrieving communications for caseId [123]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} caseId [123] UmaReclassify/BulkSetUnused function failed"));
    }

    /// <summary>
    /// Tests that if the service returns a partial success status, it returns a MultiStatus result.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ServiceReturnsPartialSuccess_ReturnsMultiStatus()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";

        var communications = new List<Communication>
        {
            new Communication(
                Id: 1,
                OriginalFileName: "file1.pdf",
                Subject: "Subject 1",
                DocumentTypeId: 1,
                MaterialId: 1001,
                Link: "http://example.com/file1",
                Status: "Pending",
                Category: "Category 1",
                Type: "Type A",
                HasAttachments: false),

            new Communication(
                Id: 2,
                OriginalFileName: "file2.pdf",
                Subject: "Subject 2",
                DocumentTypeId: 2,
                MaterialId: 1002,
                Link: "http://example.com/file2",
                Status: "Processed",
                Category: "Category 2",
                Type: "Type B",
                HasAttachments: false),
        };

        var matchedCommunications = new List<MatchedCommunication>
        {
            new MatchedCommunication { materialId = 2, subject = "Subject 2" },
        };

        // Mock the communication service to return an empty collection for communications
        this.mockCommunicationService
            .Setup(service => service.GetCommunicationsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(communications);

        // Mock the service to return matched communications
        this.mockUmaReclassifyService
            .Setup(service => service.ProcessMatchingRequest(It.IsAny<int>(), It.IsAny<IReadOnlyCollection<Communication>>()))
            .ReturnsAsync(matchedCommunications);

        // Mock the bulk set unused service to return a partial success status
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
        IActionResult result = await this.umaReclassify.Run(httpRequest, 123);

        // Assert
        ObjectResult multiStatusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status207MultiStatus, multiStatusResult.StatusCode);
        Assert.NotNull(multiStatusResult.Value);
        Assert.Equal("partial_success", ((BulkSetUnusedResponse)multiStatusResult.Value).Status);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} UmaReclassify function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Retrieving communications for caseId [123]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Warning &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [123] UmaReclassify/BulkSetUnused function completed partially successfully"));
    }


    /// <summary>
    /// Tests that when the communications retrieval fails, an exception is thrown.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task Run_CommunicationsEmpty_ReturnsNotFound()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";

        // Mock the communication service to return an empty collection
        this.mockCommunicationService
            .Setup(service => service.GetCommunicationsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(Array.Empty<Communication>());

        // Act
        IActionResult result = await this.umaReclassify.Run(httpRequest, 123);

        // Assert
        NotFoundObjectResult statusCodeResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, statusCodeResult.StatusCode);

        // Verify logging
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} UmaReclassify function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Retrieving communications for caseId [123]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} No communications found for caseId [123]"));

        // Verify that the communication service was called once
        this.mockCommunicationService.Verify(
            service =>
            service.GetCommunicationsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()), Times.Once);
    }

    /// <summary>
    /// Tests that when the matched communications response is empty,
    /// the function returns a <see cref="NotFoundObjectResult"/>.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation of the test.</returns>
    [Fact]
    public async Task Run_MatchedCommunicationsEmpty_ReturnsNotFound()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";

        var communications = new List<Communication>
        {
            new Communication(
                Id: 1,
                OriginalFileName: "file1.pdf",
                Subject: "Subject 1",
                DocumentTypeId: 1,
                MaterialId: 1001,
                Link: "http://example.com/file1",
                Status: "Pending",
                Category: "Category 1",
                Type: "Type A",
                HasAttachments: false),

            new Communication(
                Id: 2,
                OriginalFileName: "file2.pdf",
                Subject: "Subject 2",
                DocumentTypeId: 2,
                MaterialId: 1002,
                Link: "http://example.com/file2",
                Status: "Processed",
                Category: "Category 2",
                Type: "Type B",
                HasAttachments: false),
        };

        // Mock the communication service to return an empty collection for communications
        this.mockCommunicationService
            .Setup(service => service.GetCommunicationsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(communications);

        // Mock the service to return an empty collection for matched communications
        this.mockUmaReclassifyService
            .Setup(service => service.ProcessMatchingRequest(It.IsAny<int>(), It.IsAny<IReadOnlyCollection<Communication>>()))
            .ReturnsAsync(Array.Empty<MatchedCommunication>());

        // Act
        IActionResult result = await this.umaReclassify.Run(httpRequest, 123);

        // Assert
        NotFoundObjectResult notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"{LoggingConstants.HskUiLogPrefix} No matched communications found for caseId [123]", notFoundResult.Value);

        // Verify that the communication service and matching service were called
        this.mockCommunicationService.Verify(
            service => service.GetCommunicationsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()), Times.Once);

        this.mockUmaReclassifyService.Verify(
            service => service.ProcessMatchingRequest(It.IsAny<int>(), It.IsAny<IReadOnlyCollection<Communication>>()), Times.Once);

        // Verify logging
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} UmaReclassify function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Retrieving communications for caseId [123]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} No matched communications found for caseId [123]"));
    }

    /// <summary>
    /// Tests that if an exception is thrown, it returns an InternalServerError result.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [Fact]
    public async Task Run_ExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";

        var communications = new List<Communication>
        {
            new Communication(
                Id: 1,
                OriginalFileName: "file1.pdf",
                Subject: "Subject 1",
                DocumentTypeId: 1,
                MaterialId: 1001,
                Link: "http://example.com/file1",
                Status: "Pending",
                Category: "Category 1",
                Type: "Type A",
                HasAttachments: false),

            new Communication(
                Id: 2,
                OriginalFileName: "file2.pdf",
                Subject: "Subject 2",
                DocumentTypeId: 2,
                MaterialId: 1002,
                Link: "http://example.com/file2",
                Status: "Processed",
                Category: "Category 2",
                Type: "Type B",
                HasAttachments: false),
        };

        // Mock the communication service to return an empty collection for communications
        this.mockCommunicationService
            .Setup(service => service.GetCommunicationsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(communications);

        // Mock the uma reclassification service throwing an exception
        this.mockUmaReclassifyService
            .Setup(service => service.ProcessMatchingRequest(It.IsAny<int>(), It.IsAny<IReadOnlyCollection<Communication>>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        IActionResult result = await this.umaReclassify.Run(httpRequest, 123);

        // Assert
        StatusCodeResult statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

        // Verify logging
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} UmaReclassify function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Retrieving communications for caseId [123]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} UmaReclassify function encountered an error: Unexpected error"));
    }
}
