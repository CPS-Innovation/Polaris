// <copyright file="DocumentServiceTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services.Tests;

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using Cps.Fct.Hk.Ui.Interfaces;
using Azure.Storage.Blobs;
using DdeiClient.Clients.Interfaces;
using Microsoft.Extensions.Configuration;
using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Configuration;
using Common.Constants;

/// <summary>
/// Unit tests for the <see cref="DocumentService"/> class.
/// </summary>
public class DocumentServiceTests
{
    private readonly TestLogger<DocumentService> mockLogger;
    private readonly Mock<IMasterDataServiceClient> mockApiClient;
    private readonly Mock<IConversionService> mockConversionService;
    private readonly Mock<BlobServiceClient> mockBlobServiceClient;
    private readonly DocumentService documentService;
    private readonly Mock<IConfiguration> mockConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentServiceTests"/> class.
    /// Sets up the mock logger and API client for the tests.
    /// </summary>
    public DocumentServiceTests()
    {
        this.mockLogger = new TestLogger<DocumentService>();
        this.mockApiClient = new Mock<IMasterDataServiceClient>();
        this.mockConversionService = new Mock<IConversionService>();
        this.mockBlobServiceClient = new Mock<BlobServiceClient>();
        this.mockConfiguration = new Mock<IConfiguration>();
        this.documentService = new DocumentService(
            this.mockLogger,
            this.mockApiClient.Object,
            this.mockConversionService.Object,
            this.mockConfiguration.Object,
            this.mockBlobServiceClient.Object);
    }

    /// <summary>
    /// Tests <see cref="DocumentService.GetMaterialDocumentAsync(string, string, CmsAuthValues, bool)"/>
    /// when the document is successfully downloaded, converted to PDF, and stored in Blob Storage.
    /// Expects a valid <see cref="FileStreamResult"/> object to be returned with a PDF content type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetMaterialDocumentAsync_ValidDocument_ReturnsFileStreamResult()
    {
        // Arrange
        string caseId = "123";
        string link = "/case/material/document.txt";
        var cmsAuthValues = new CmsAuthValues("valid cookies", "valid token", Guid.NewGuid());
        var mockFileStreamResult = new FileStreamResult(new MemoryStream(), "text/plain") { FileDownloadName = "preview_document" };
        string blobContainerName = "test-container";
        string pdfBlobUrl = "https://blobstorage/test/preview_document.pdf";

        Environment.SetEnvironmentVariable("BlobContainerName", blobContainerName);

        // Mock the document download from API client
        this.mockApiClient
            .Setup(client => client.GetMaterialDocumentAsync(It.IsAny<GetDocumentRequest>(), cmsAuthValues))
            .ReturnsAsync(mockFileStreamResult);

        // Mock the behavior of saving the document to temporary storage
        this.mockConversionService
            .Setup(service => service.SaveDownloadedDocumentToTemporaryStorageAsync(It.IsAny<FileStreamResult>()))
            .ReturnsAsync(true);

        // Mock the PDF conversion service
        this.mockConversionService
            .Setup(service => service.ConvertToPdfDocumentAsync(It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(pdfBlobUrl);

        // Mock the BlobClient to simulate blob storage interaction
        var mockBlobClient = new Mock<BlobClient>();
        mockBlobClient
            .Setup(blob => blob.DownloadToAsync(It.IsAny<Stream>()))
            .Callback<Stream>(stream =>
            {
                byte[] content = { 1, 2, 3, 4 };
                stream.Write(content, 0, content.Length);
                if (stream is MemoryStream memoryStream)
                {
                    memoryStream.Position = 0; // Reset position for reading
                }
            })
            .Returns(Task.FromResult(Mock.Of<Azure.Response>())); // Return a Task<Azure.Response>

        var mockBlobContainerClient = new Mock<BlobContainerClient>();
        mockBlobContainerClient
            .Setup(container => container.GetBlobClient(It.IsAny<string>()))
            .Returns(mockBlobClient.Object);

        this.mockBlobServiceClient
            .Setup(client => client.GetBlobContainerClient(blobContainerName))
            .Returns(mockBlobContainerClient.Object);

        this.mockConfiguration
       .Setup(c => c[StorageKeys.BlobServiceContainerNameDocuments])
       .Returns(blobContainerName);

        // Act
        var result = await this.documentService.GetMaterialDocumentAsync(caseId, link, cmsAuthValues);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<FileStreamResult>(result);
        Assert.Equal("application/pdf", result.ContentType);
        Assert.Equal("preview_document.pdf", result.FileDownloadName);

        // Verify that logs contain the download operation
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Downloading document with file path [{link}] ..."));
    }

    /// <summary>
    /// Tests <see cref="DocumentService.GetMaterialDocumentAsync(string, string, CmsAuthValues, bool)"/>
    /// when no document is found. Expects an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetMaterialDocumentAsync_DocumentNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        string caseId = "123";
        string link = "/case/material/missing_document.txt";
        var cmsAuthValues = new CmsAuthValues("valid cookies", "valid token", Guid.NewGuid());

        this.mockApiClient
            .Setup(client => client.GetMaterialDocumentAsync(It.IsAny<GetDocumentRequest>(), cmsAuthValues))
            .ReturnsAsync((FileStreamResult?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => this.documentService.GetMaterialDocumentAsync(caseId, link, cmsAuthValues, false));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Downloading document with file path [/case/material/missing_document.txt] ..."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Error occurred while downloading document with file path [/case/material/missing_document.txt]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} No case material document found for file path [/case/material/missing_document.txt]"));
    }

    /// <summary>
    /// Tests <see cref="DocumentService.GetMaterialDocumentAsync(string, string, CmsAuthValues, bool)"/>
    /// when an exception occurs during document retrieval. Expects the exception to be thrown.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetMaterialDocumentAsync_ExceptionThrown_ThrowsException()
    {
        // Arrange
        string caseId = "123";
        string link = "/case/material/document.txt";
        var cmsAuthValues = new CmsAuthValues("valid cookies", "valid token", Guid.NewGuid());
        var exception = new Exception("DDEI API error");

        this.mockApiClient
            .Setup(client => client.GetMaterialDocumentAsync(It.IsAny<GetDocumentRequest>(), cmsAuthValues))
            .ThrowsAsync(exception);

        // Act & Assert
        Exception ex = await Assert.ThrowsAsync<Exception>(() => this.documentService.GetMaterialDocumentAsync(caseId, link, cmsAuthValues));
        Assert.Equal("DDEI API error", ex.Message);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Downloading document with file path [/case/material/document.txt] ..."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Error occurred while downloading document with file path [/case/material/document.txt]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"DDEI API error"));
    }

    /// <summary>
    /// Tests <see cref="DocumentService.GetMaterialDocumentAsync(string, string, CmsAuthValues, bool)"/>
    /// when an not supported exception occurs during document retrieval. Expects the exception to be thrown.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetMaterialDocumentAsync_NotSupportedExceptionThrown_ThrowsNotSupportedException()
    {
        // Arrange
        string caseId = "123";
        string link = "/case/material/document.txt";
        var cmsAuthValues = new CmsAuthValues("valid cookies", "valid token", Guid.NewGuid());
        var exception = new NotSupportedException("DDEI API error");

        this.mockApiClient
            .Setup(client => client.GetMaterialDocumentAsync(It.IsAny<GetDocumentRequest>(), cmsAuthValues))
            .ThrowsAsync(exception);

        // Act & Assert
        NotSupportedException ex = await Assert.ThrowsAsync<NotSupportedException>(() => this.documentService.GetMaterialDocumentAsync(caseId, link, cmsAuthValues));
        Assert.Equal("DDEI API error", ex.Message);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Downloading document with file path [/case/material/document.txt] ..."));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Error: {ex.Message} for file path [/case/material/document.txt]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"DDEI API error"));
    }

    /// <summary>
    /// Tests <see cref="DocumentService.UrlEncodeFilePath(string)"/> with a valid file path.
    /// Expects the first part to be URL double-encoded as '%252F'.
    /// </summary>
    [Fact]
    public void UrlEncodeFilePath_ValidFilePath_DoubleEncodesFirstPart()
    {
        // Arrange
        string filePath = "documents/file.txt";

        // Act
        string result = this.documentService.UrlEncodeFilePath(filePath);

        // Assert
        Assert.Equal("%252Fdocuments/file.txt", result);
    }

    /// <summary>
    /// Tests <see cref="DocumentService.UrlEncodeFilePath(string)"/> to ensure the first part is double-encoded.
    /// Expects the first part to be URL double-encoded as '%252F'.
    /// </summary>
    [Fact]
    public void UrlEncodeFilePath_DoubleEncodesTheFirstPart()
    {
        // Arrange
        string filePath = "file.txt";

        // Act
        string result = this.documentService.UrlEncodeFilePath(filePath);

        // Assert
        Assert.Equal("%252Ffile.txt", result);
    }

    /// <summary>
    /// Tests <see cref="DocumentService.UrlEncodeFilePath(string)"/> when the input is null or whitespace.
    /// Expects an <see cref="ArgumentException"/> to be thrown.
    /// </summary>
    /// <param name="filePath">The file path that is null, empty, or whitespace, which will cause the method to throw an exception.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void UrlEncodeFilePath_NullOrEmptyPath_ThrowsArgumentException(string? filePath)
    {
        // Act & Assert
        ArgumentException ex = Assert.Throws<ArgumentException>(() => this.documentService.UrlEncodeFilePath(filePath!));
        Assert.Equal($"{LoggingConstants.HskUiLogPrefix} File path cannot be null or empty (Parameter 'filePath')", ex.Message);
    }
}
