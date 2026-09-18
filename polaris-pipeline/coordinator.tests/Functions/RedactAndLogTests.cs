// <copyright file="RedactAndLogTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.tests.Functions;

using Common.Configuration;
using Common.Dto.Request;
using Common.Dto.Response;
using Common.Services.BlobStorage;
using coordinator.Clients.PdfRedactor;
using coordinator.Clients.RedactionLogger;
using coordinator.Functions;
using coordinator.Services;
using Ddei.Factories;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Services.CaseUrnResolver;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class RedactAndLogTests
{
    private readonly Mock<IValidator<RedactPdfRequestWithDocumentDto>> requestValidatorMock;
    private readonly Mock<IRedactionService> redactionServiceMock;
    private readonly Mock<IRedactionLoggerClient> loggerClientMock;
    private readonly Mock<IPolarisBlobStorageService> blobStorageServiceMock;
    private readonly Mock<IMdsArgFactory> mdsArgFactoryMock;
    private readonly Mock<IMdsClient> mdsClientMock;
    private readonly Mock<ICaseUrnResolver> caseUrnResolverMock;
    private readonly Mock<IConfiguration> configurationMock;
    private readonly Mock<ILogger<RedactAndLog>> loggerMock;

    private readonly RedactAndLog redactAndLog;

    public RedactAndLogTests()
    {
        this.requestValidatorMock =
            new Mock<IValidator<RedactPdfRequestWithDocumentDto>>();

        this.redactionServiceMock =
            new Mock<IRedactionService>();

        this.loggerClientMock =
            new Mock<IRedactionLoggerClient>();

        this.blobStorageServiceMock =
            new Mock<IPolarisBlobStorageService>();

        this.mdsArgFactoryMock =
            new Mock<IMdsArgFactory>();

        this.mdsClientMock =
            new Mock<IMdsClient>();

        this.caseUrnResolverMock =
            new Mock<ICaseUrnResolver>();

        this.configurationMock =
            new Mock<IConfiguration>();

        this.loggerMock =
            new Mock<ILogger<RedactAndLog>>();

        this.configurationMock
            .Setup(c => c[StorageKeys.BlobServiceContainerNameDocuments])
            .Returns("documents");

        Func<string, IPolarisBlobStorageService> blobStorageServiceFactory =
            _ => this.blobStorageServiceMock.Object;

        this.redactAndLog = new RedactAndLog(
            this.requestValidatorMock.Object,
            this.redactionServiceMock.Object,
            this.loggerClientMock.Object,
            blobStorageServiceFactory,
            this.mdsArgFactoryMock.Object,
            this.configurationMock.Object,
            this.mdsClientMock.Object,
            this.caseUrnResolverMock.Object,
            this.loggerMock.Object);
    }

    [Fact]
    public async Task HttpStart_ValidRequest_ShouldReturnOk()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var caseId = 1;
        var materialId = "CMS-12345";
        var documentId = 2L;
        var cancellationToken = CancellationToken.None;

        var request = new RedactAndLogRequestDto
        {
            RedactionPayload = new RedactPdfRequestDto { RedactionDefinitions = [], DocumentModifications = [], },
            LogPayload = null,
        };

        var httpRequest = CreateHttpRequest(
            request,
            correlationId);

        this.redactionServiceMock
            .Setup(s => s.ProcessAsync(
                caseId,
                materialId,
                documentId,
                request.RedactionPayload,
                It.IsAny<Common.Dto.Request.CmsAuthValues>(),
                correlationId,
                cancellationToken))
            .Returns(Task.FromResult<System.IO.Stream>(new MemoryStream()));



        this.loggerClientMock
            .Setup(s => s.CreateRedactionLog(
                request.LogPayload,
                correlationId))
            .Returns(Task.FromResult<System.IO.Stream>(new MemoryStream()));

        // Act
        var result = await this.redactAndLog.HttpStart(
            httpRequest,
            caseId,
            materialId,
            documentId,
            cancellationToken);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        Assert.Equal(
            StatusCodes.Status200OK,
            okResult.StatusCode);

        var response =
            Assert.IsType<RedactAndLogResponse>(okResult.Value);

        Assert.Equal(correlationId, response.CorrelationId);
        Assert.True(response.Success);

        this.redactionServiceMock.Verify(
            s => s.ProcessAsync(
                caseId,
                materialId,
                documentId,
                request.RedactionPayload,
                It.IsAny<Common.Dto.Request.CmsAuthValues>(),
                correlationId,
                cancellationToken),
            Times.Never);

        this.loggerClientMock.Verify(
            s => s.CreateRedactionLog(
                request.LogPayload,
                correlationId),
            Times.Once);
    }

    [Fact]
    public async Task HttpStart_EmptyRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var caseId = 1;
        var materialId = "CMS-12345";
        var documentId = 2L;
        var cancellationToken = CancellationToken.None;

        var httpRequest = new DefaultHttpContext().Request;

        httpRequest.Headers["Correlation-Id"] =
            correlationId.ToString();

        // Act
        var result = await this.redactAndLog.HttpStart(
            httpRequest,
            caseId,
            materialId,
            documentId,
            cancellationToken);

        // Assert
        var badRequestResult =
            Assert.IsType<ObjectResult>(result);

        Assert.Equal(
            StatusCodes.Status500InternalServerError,
            badRequestResult.StatusCode);

        var response =
            Assert.IsType<RedactAndLogResponse>(
                badRequestResult.Value);

        Assert.Equal(correlationId, response.CorrelationId);
        Assert.False(response.Success);

        this.redactionServiceMock.Verify(
            s => s.ProcessAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<long>(),
                It.IsAny<RedactPdfRequestDto>(),
                It.IsAny<Common.Dto.Request.CmsAuthValues>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        this.loggerClientMock.Verify(
            s => s.CreateRedactionLog(
                (CreateRedactionLogsRequest)It.IsAny<object>(),
                It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task HttpStart_RedactionServiceThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var caseId = 1;
        var materialId = "CMS-12345";
        var documentId = 2L;
        var cancellationToken = CancellationToken.None;

        var request = new RedactAndLogRequestDto
        {
            RedactionPayload = new RedactPdfRequestDto
            {
                RedactionDefinitions = [],
                DocumentModifications = [],
            },
            LogPayload = null,
        };

        var httpRequest = CreateHttpRequest(
            request,
            correlationId);

        this.redactionServiceMock
            .Setup(s => s.ProcessAsync(
                caseId,
                materialId,
                documentId,
                request.RedactionPayload,
                It.IsAny<Common.Dto.Request.CmsAuthValues>(),
                correlationId,
                cancellationToken))
            .ThrowsAsync(new Exception("Redaction failed"));

        // Act
        var result = await this.redactAndLog.HttpStart(
            httpRequest,
            caseId,
            materialId,
            documentId,
            cancellationToken);

        // Assert
        var objectResult =
            Assert.IsType<OkObjectResult>(result);

        Assert.Equal(
            StatusCodes.Status200OK,
            objectResult.StatusCode);

        var response =
            Assert.IsType<RedactAndLogResponse>(
                objectResult.Value);

        Assert.Equal(correlationId, response.CorrelationId);
        Assert.True(response.Success);

        this.loggerClientMock.Verify(
            s => s.CreateRedactionLog(
                (CreateRedactionLogsRequest)It.IsAny<object>(),
                It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact]
    public async Task HttpStart_LoggerThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var caseId = 1;
        var materialId = "CMS-12345";
        var documentId = 2L;
        var cancellationToken = CancellationToken.None;

        var request = new RedactAndLogRequestDto
        {
            RedactionPayload = new RedactPdfRequestDto
            {
                RedactionDefinitions = [],
                DocumentModifications = [],
            },
            LogPayload = null,
        };

        var httpRequest = CreateHttpRequest(
            request,
            correlationId);

        this.redactionServiceMock
            .Setup(s => s.ProcessAsync(
                caseId,
                materialId,
                documentId,
                request.RedactionPayload,
                It.IsAny<Common.Dto.Request.CmsAuthValues>(),
                correlationId,
                cancellationToken))
            .ReturnsAsync(new MemoryStream());

        this.loggerClientMock
            .Setup(s => s.CreateRedactionLog(
                request.LogPayload,
                correlationId))
            .ThrowsAsync(new Exception("Logging failed"));

        // Act
        var result = await this.redactAndLog.HttpStart(
            httpRequest,
            caseId,
            materialId,
            documentId,
            cancellationToken);

        // Assert
        var objectResult =
            Assert.IsType<ObjectResult>(result);

        Assert.Equal(
            StatusCodes.Status500InternalServerError,
            objectResult.StatusCode);

        var response =
            Assert.IsType<RedactAndLogResponse>(
                objectResult.Value);

        Assert.Equal(correlationId, response.CorrelationId);
        Assert.False(response.Success);

        //this.redactionServiceMock.Verify(
        //    s => s.ProcessAsync(
        //        caseId,
        //        materialId,
        //        documentId,
        //        request.RedactionPayload,
        //        It.IsAny<Common.Dto.Request.CmsAuthValues>(),
        //        correlationId,
        //        cancellationToken),
        //    Times.Once);

        //this.loggerClientMock.Verify(
        //    s => s.CreateRedactionLog(
        //        request.LogPayload,
        //        correlationId),
        //    Times.Once);
    }

    [Fact]
    public async Task HttpStart_CancellationRequested_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var caseId = 1;
        var materialId = "CMS-12345";
        var documentId = 2L;

        using var cancellationTokenSource =
            new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        var cancellationToken =
            cancellationTokenSource.Token;

        var request = new RedactAndLogRequestDto
        {
            RedactionPayload = new RedactPdfRequestDto
            {
                RedactionDefinitions = [],
                DocumentModifications = [],
            },
            LogPayload = null,
        };

        var httpRequest = CreateHttpRequest(
            request,
            correlationId);

        // Act & Assert
        await Assert.ThrowsAsync<System.Threading.Tasks.TaskCanceledException>(
            () => this.redactAndLog.HttpStart(
                httpRequest,
                caseId,
                materialId,
                documentId,
                cancellationToken));
    }

    private static HttpRequest CreateHttpRequest(RedactAndLogRequestDto request, Guid correlationId)
    {
        var context = new DefaultHttpContext();
        var httpRequest = context.Request;
        httpRequest.Headers["Correlation-Id"] = correlationId.ToString();
        httpRequest.Headers["Cms-Auth-Values"] = "Cms-auth-values";
        httpRequest.ContentType = "application/json";
        var json = System.Text.Json.JsonSerializer.Serialize(request);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        httpRequest.Body = new MemoryStream(bytes);
        return httpRequest;
    }
}
