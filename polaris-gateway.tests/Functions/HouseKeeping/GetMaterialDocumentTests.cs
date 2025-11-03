// <copyright file="GetMaterialDocumentTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions.HouseKeeping;

using System;
using System.Collections.Generic;
using System.IO;
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
/// Unit tests for the <see cref="GetMaterialDocument"/> class.
/// </summary>
public class GetMaterialDocumentTests
{
    private readonly TestLogger<GetMaterialDocument> mockLogger;
    private readonly Mock<ICommunicationService> mockCommunicationService;
    private readonly Mock<IDocumentService> mockDocumentService;
    private readonly GetMaterialDocument getMaterialDocumentunction;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMaterialDocumentTests"/> class.
    /// </summary>
    public GetMaterialDocumentTests()
    {
        // Initialize mocks
        this.mockLogger = new TestLogger<GetMaterialDocument>();
        this.mockCommunicationService = new Mock<ICommunicationService>();
        this.mockDocumentService = new Mock<IDocumentService>();

        // Initialize the function class
        this.getMaterialDocumentunction = new GetMaterialDocument(
            this.mockLogger,
            this.mockCommunicationService.Object,
            this.mockDocumentService.Object);
    }

    /// <summary>
    /// Tests that Run returns a BadRequestObjectResult for invalid material_id format.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_InvalidMaterialId_ReturnsBadRequest()
    {
        // Arrange
        HttpRequest req = new DefaultHttpContext().Request;
        int invalidMaterialId = 0;

        // Act
        IActionResult result = await this.getMaterialDocumentunction.Run(req, 123, invalidMaterialId);

        // Assert
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetMaterialDocument function processed a request."));

        BadRequestObjectResult badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal($"{LoggingConstants.HskUiLogPrefix} Invalid material_id format. It should be an integer.", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that Run returns a NotFoundObjectResult when no valid link is found for the case material document.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_NoValidLinkFound_ReturnsNotFound()
    {
        // Arrange
        HttpRequest req = new DefaultHttpContext().Request;
        int validMaterialId = 12;

        this.mockCommunicationService
            .Setup(s => s.GetCommunicationsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(new List<Communication>());

        // Act
        IActionResult result = await this.getMaterialDocumentunction.Run(req, 123, validMaterialId);

        // Assert
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetMaterialDocument function processed a request."));

        NotFoundObjectResult notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"{LoggingConstants.HskUiLogPrefix} No valid link found for the case material document with materialId [12].", notFoundResult.Value);
    }

    /// <summary>
    /// Tests the Run method to ensure that when document retrieval fails, it returns a <see cref="FileStreamResult"/>
    /// with a specific file download name.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_DocumentRetrievalFails_ReturnsNotFoundFileStreamResult_WithFileDownloadName()
    {
        // Arrange
        HttpRequest req = new DefaultHttpContext().Request;
        int validMaterialId = 121;

        this.mockCommunicationService.Setup(s => s.GetCommunicationsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(new List<Communication>
            {
                new Communication(1, "File1.pdf", "Subject A", 123, 1, "http://example.com/document.pdf", "None", "Administrative", "Type A", false),
            });

        this.mockCommunicationService
            .Setup(s => s.GetCaseMaterialLinkAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync("http://example.com/document.pdf");

        // Simulate document retrieval failure
        this.mockDocumentService.Setup(s => s.GetMaterialDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), false))
            .ReturnsAsync((FileStreamResult)null);

        // Act
        IActionResult result = await this.getMaterialDocumentunction.Run(req, 123, validMaterialId);

        // Assert that the result is a FileStreamResult
        FileStreamResult fileStreamResult = Assert.IsType<FileStreamResult>(result);

        // Assert the FileDownloadName is correct
        Assert.Equal("not_found.txt", fileStreamResult.FileDownloadName);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetMaterialDocument function processed a request."));
    }

    /// <summary>
    /// Tests that Run returns a NotSupportException when an unsupported content type is encountered.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ShouldReturnUnprocessableEntity_WhenNotSupportedExceptionThrown()
    {
        // Arrange
        HttpRequest req = new DefaultHttpContext().Request;
        int validMaterialId = 12;

        this.mockCommunicationService.Setup(s => s.GetCommunicationsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(new List<Communication>
            {
                new Communication(1, "File1.unsupported", "Subject A", 123, 1, "http://example.com/document.unsupported", "None", "Administrative", "Type A", false),
            });

        this.mockCommunicationService
            .Setup(s => s.GetCaseMaterialLinkAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync("http://example.com/document.unsupported");

        // Simulate throwing a NotSupportedException during document retrieval
        this.mockDocumentService.Setup(s => s.GetMaterialDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), true))
            .ThrowsAsync(new NotSupportedException("Unsupported content type"));

        // Act
        IActionResult result = await this.getMaterialDocumentunction.Run(req, 123, validMaterialId);

        // Assert that the result is an UnprocessableEntityObjectResult
        UnprocessableEntityObjectResult unprocessableEntityResult = Assert.IsType<UnprocessableEntityObjectResult>(result);

        // Assert the FileDownloadName is correct
        Assert.Equal("Preview error: Unsupported content type", unprocessableEntityResult.Value);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetMaterialDocument function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetMaterialDocument function encountered unsupported content type."));
    }

    /// <summary>
    /// Tests that Run returns the document when successfully retrieved.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_SuccessfulDocumentRetrieval_ReturnsDocument()
    {
        // Arrange
        HttpRequest req = new DefaultHttpContext().Request;
        int validMaterialId = 12;

        this.mockCommunicationService
            .Setup(s => s.GetCommunicationsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(new List<Communication>
            {
                new Communication(1, "File1.pdf", "Subject A", 123, 1, "http://example.com/document.pdf", "None", "Administrative", "Type A", false),
            });

        this.mockCommunicationService
            .Setup(s => s.GetCaseMaterialLinkAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync("http://example.com/document.pdf");

        var memoryStream = new MemoryStream();
        var fileStreamResult = new FileStreamResult(memoryStream, "application/pdf")
        {
            FileDownloadName = "document.pdf",
        };

        this.mockDocumentService.Setup(s => s.GetMaterialDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), true))
            .ReturnsAsync(fileStreamResult);

        // Act
        IActionResult result = await this.getMaterialDocumentunction.Run(req, 123, validMaterialId);

        // Assert
        FileStreamResult fileResult = Assert.IsType<FileStreamResult>(result);
        Assert.NotNull(fileResult);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Equal("document.pdf", fileResult.FileDownloadName);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetMaterialDocument function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [123] GetMaterialDocument function completed"));
    }
}
