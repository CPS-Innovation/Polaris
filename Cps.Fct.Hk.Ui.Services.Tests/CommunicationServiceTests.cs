// <copyright file="CommunicationServiceTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services.Tests;

using Cps.Fct.Hk.Ui.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using DdeiClient.Clients.Interfaces;
using Common.Dto.Response.HouseKeeping;
using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Constants;

using ApiClient = Cps.MasterDataService.Infrastructure.ApiClient;
using Common.Enums;

/// <summary>
/// Unit tests for the <see cref="CommunicationService"/> class.
/// </summary>
public class CommunicationServiceTests
{
    private readonly TestLogger<CommunicationService> mockLogger;
    private readonly Mock<IMasterDataServiceClient> apiClientMock;
    private readonly Mock<IDocumentTypeMapper> documentTypeMapperMock;
    private readonly Mock<ICommunicationMapper> communicationMapperMock;
    private readonly CommunicationService communicationService;
    private readonly DateTime receivedDate = new DateTime(2025, 04, 01);
    private readonly DateTime statementTakenDate = new DateTime(2025, 03, 02);

    /// <summary>
    /// Initializes a new instance of the <see cref="CommunicationServiceTests"/> class.
    /// Sets up mock dependencies and initializes the <see cref="CommunicationService"/> to be tested.
    /// </summary>
    public CommunicationServiceTests()
    {
        this.mockLogger = new TestLogger<CommunicationService>();
        this.apiClientMock = new Mock<IMasterDataServiceClient>();
        this.documentTypeMapperMock = new Mock<IDocumentTypeMapper>();
        this.communicationMapperMock = new Mock<ICommunicationMapper>();
        this.communicationService = new CommunicationService(
            this.mockLogger,
            this.apiClientMock.Object,
            this.documentTypeMapperMock.Object,
            this.communicationMapperMock.Object);
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.GetCommunicationsAsync(int, CmsAuthValues)"/>
    /// returns mapped communications when the API returns valid communications.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetCommunicationsAsync_ShouldReturnMappedCommunications_WhenApiReturnsCommunications()
    {
        // Arrange
        int caseId = 1234;
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);
        DateTime date = new DateTime(2021, 10, 12);
        var apiCommunications = new List<Communication>
        {
            new Communication(1, "FileA.pdf", "Subject A", 1012, 123, "/some/path/doc1.pdf", "Open", "Classified", "Type A", false, "Email", "Incoming", "POL", date),
            new Communication(2, "FileB.pdf", "Subject B", 1034, 456, "/some/path/doc2.pdf", "Closed", "Confidential", "Type B", false, "Email", "Incoming", "POL", date),
        };

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        this.apiClientMock
            .Setup(client => client.ListCommunicationsHkAsync(It.IsAny<ListCommunicationsHkRequest>(), cmsAuthValues))
            .ReturnsAsync(apiCommunications);

        this.documentTypeMapperMock
            .Setup(mapper => mapper.MapDocumentType(1012))
            .Returns(new DocumentTypeInfo { Category = "Mapped Category A", DocumentType = "Mapped DocumentType A" });

        this.documentTypeMapperMock
            .Setup(mapper => mapper.MapDocumentType(1034))
            .Returns(new DocumentTypeInfo { Category = "Mapped Category B", DocumentType = "Mapped DocumentType B" });

        // Act
        IReadOnlyCollection<Communication> result = await this.communicationService.GetCommunicationsAsync(caseId, cmsAuthValues);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Communication firstCommunication = result.First();
        Assert.Equal("Mapped DocumentType A", firstCommunication.Type);
        Assert.Equal("Mapped Category A", firstCommunication.Category);

        Communication secondCommunication = result.Last();
        Assert.Equal("Mapped DocumentType B", secondCommunication.Type);
        Assert.Equal("Mapped Category B", secondCommunication.Category);

        this.apiClientMock.Verify(client => client.ListCommunicationsHkAsync(It.Is<ListCommunicationsHkRequest>(r => r.CaseId == caseId), cmsAuthValues), Times.Once);
        this.communicationMapperMock.Verify(mapper => mapper.MapCommunicationMethod(It.IsAny<string>()), Times.Exactly(2));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Fetching inbox communications for caseId [{caseIdString}]"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.GetCommunicationsAsync(int, CmsAuthValues)"/>
    /// logs an error and rethrows the exception when the API throws an exception.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetCommunicationsAsync_ShouldLogErrorAndRethrow_WhenApiThrowsException()
    {
        // Arrange
        int caseId = 1234;
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);
        var exception = new Exception("API Error");

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        this.apiClientMock
            .Setup(client => client.ListCommunicationsHkAsync(It.IsAny<ListCommunicationsHkRequest>(), cmsAuthValues))
            .ThrowsAsync(exception);

        // Act & Assert
        Exception ex = await Assert.ThrowsAsync<Exception>(() => this.communicationService.GetCommunicationsAsync(caseId, cmsAuthValues));
        Assert.Equal("API Error", ex.Message);

        this.apiClientMock.Verify(client => client.ListCommunicationsHkAsync(It.Is<ListCommunicationsHkRequest>(r => r.CaseId == caseId), cmsAuthValues), Times.Once);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Fetching inbox communications for caseId [{caseIdString}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching communications for caseId [{caseIdString}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"API Error"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.GetCommunicationsAsync(int, CmsAuthValues)"/>
    /// maps the document type information returned by the API correctly.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetCommunicationsAsync_ShouldMapDocumentTypeInfo_WhenApiReturnsCommunications()
    {
        // Arrange
        int caseId = 1234;
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);
        var apiCommunications = new List<Communication>
        {
            new Communication(1, "FileA.pdf", "Subject A", 1034, 123, "/some/path/doc1.pdf", "Used", "Statement", "Type A", false, "Email", "Incoming", "POL", new DateTime(25, 1, 1)),
        };

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        this.apiClientMock
            .Setup(client => client.ListCommunicationsHkAsync(It.IsAny<ListCommunicationsHkRequest>(), cmsAuthValues))
            .ReturnsAsync(apiCommunications);

        this.documentTypeMapperMock
            .Setup(mapper => mapper.MapDocumentType(1034))
            .Returns(new DocumentTypeInfo { Category = "Mapped Category A", DocumentType = "Mapped DocumentType A" });

        // Act
        IReadOnlyCollection<Communication> result = await this.communicationService.GetCommunicationsAsync(caseId, cmsAuthValues);

        // Assert
        Assert.Single(result);

        Communication firstCommunication = result.First();
        Assert.Equal("Mapped DocumentType A", firstCommunication.Type);
        Assert.Equal("Mapped Category A", firstCommunication.Category);
        Assert.Equal("Used", firstCommunication.Status);

        this.documentTypeMapperMock.Verify(mapper => mapper.MapDocumentType(1034), Times.Once);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Fetching inbox communications for caseId [{caseIdString}]"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.GetUsedStatementsAsync(int, CmsAuthValues)"/>
    /// returns used statements when the API returns valid used statements.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetUsedStatementsAsync_ShouldReturn_WhenApiReturnsUsedStatements()
    {
        // Arrange
        int caseId = 1234;
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);

        var statement1 = new Statement(
                Id: 1,
                WitnessId: 789,
                Title: "Statement One",
                OriginalFileName: "statement1.pdf",
                MaterialType: "1202",
                DocumentType: 2,
                Link: "http://example1.com",
                Status: "Pending",
                ReceivedDate: this.receivedDate,
                StatementTakenDate: this.statementTakenDate);

        var statement2 = new Statement(
                Id: 2,
                WitnessId: null,
                Title: "Statement Two",
                OriginalFileName: "statement2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "Pending",
                ReceivedDate: this.receivedDate,
                StatementTakenDate: this.statementTakenDate);

        var statements = new List<Statement>
        {
            statement1,
            statement2,
        };

        var apiUsedStatements = new UsedStatementsResponse
        {
            Statements = statements,
        };

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        this.apiClientMock
            .Setup(client => client.GetUsedStatementsAsync(It.IsAny<GetUsedStatementsRequest>(), cmsAuthValues))
            .ReturnsAsync(apiUsedStatements);

        // Act
        UsedStatementsResponse result = await this.communicationService.GetUsedStatementsAsync(caseId, cmsAuthValues);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Statements);
        Assert.Equal(2, result.Statements.Count);
        Assert.Equal("statement1.pdf", result.Statements[0].OriginalFileName);
        Assert.Equal("statement2.pdf", result.Statements[1].OriginalFileName);
        Assert.Equal(this.receivedDate, result.Statements[1].ReceivedDate);
        Assert.Equal(this.statementTakenDate, result.Statements[1].StatementTakenDate);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Fetching used statements for caseId [{caseIdString}]"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.GetUsedStatementsAsync(int, CmsAuthValues)"/>
    /// logs an error and rethrows the exception when the API throws an exception.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetUsedStatementsAsync_ShouldLogErrorAndRethrow_WhenApiThrowsException()
    {
        // Arrange
        int caseId = 1234;
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);
        var exception = new Exception("API Error");

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        this.apiClientMock
            .Setup(client => client.GetUsedStatementsAsync(It.IsAny<GetUsedStatementsRequest>(), cmsAuthValues))
            .ThrowsAsync(exception);

        // Act & Assert
        Exception ex = await Assert.ThrowsAsync<Exception>(() => this.communicationService.GetUsedStatementsAsync(caseId, cmsAuthValues));
        Assert.Equal("API Error", ex.Message);

        this.apiClientMock.Verify(client => client.GetUsedStatementsAsync(It.Is<GetUsedStatementsRequest>(r => r.CaseId == caseId), cmsAuthValues), Times.Once);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Fetching used statements for caseId [{caseIdString}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching used statements for caseId [{caseIdString}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains("API Error"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.GetUsedExhibitsAsync(int, CmsAuthValues)"/>
    /// returns used exhibits when the API returns valid used exhibits.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetUsedExhibitsAsync_ShouldReturn_WhenApiReturnsUsedExhibits()
    {
        // Arrange
        int caseId = 1234;
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);

        var exhibit1 = new Exhibit(
                Id: 1,
                Title: "Exhibit One",
                OriginalFileName: "exhibit1.pdf",
                MaterialType: "1202",
                DocumentType: 2,
                Link: "http://example1.com",
                Status: "Pending",
                ReceivedDate: this.receivedDate,
                Reference: "some-reference",
                Producer: "some-producer");

        var exhibit2 = new Exhibit(
                Id: 2,
                Title: "Exhibit Two",
                OriginalFileName: "exhibit2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "Complete",
                ReceivedDate: this.receivedDate,
                Reference: "some-reference",
                Producer: "some-producer");

        var exhibits = new List<Exhibit>
        {
            exhibit1,
            exhibit2,
        };

        var apiUsedExhibits = new UsedExhibitsResponse
        {
            Exhibits = exhibits,
        };

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        this.apiClientMock
            .Setup(client => client.GetUsedExhibitsAsync(It.IsAny<GetUsedExhibitsRequest>(), cmsAuthValues))
            .ReturnsAsync(apiUsedExhibits);

        // Act
        UsedExhibitsResponse result = await this.communicationService.GetUsedExhibitsAsync(caseId, cmsAuthValues);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Exhibits);
        Assert.Equal(2, result.Exhibits.Count);
        Assert.Equal("exhibit1.pdf", result.Exhibits[0].OriginalFileName);
        Assert.Equal("exhibit2.pdf", result.Exhibits[1].OriginalFileName);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Fetching used exhibits for caseId [{caseIdString}]"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.GetUsedExhibitsAsync(int, CmsAuthValues)"/>
    /// logs an error and rethrows the exception when the API throws an exception.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetUsedExhibitsAsync_ShouldLogErrorAndRethrow_WhenApiThrowsException()
    {
        // Arrange
        int caseId = 1234;
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);
        var exception = new Exception("API Error");

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        this.apiClientMock
            .Setup(client => client.GetUsedExhibitsAsync(It.IsAny<GetUsedExhibitsRequest>(), cmsAuthValues))
            .ThrowsAsync(exception);

        // Act & Assert
        Exception ex = await Assert.ThrowsAsync<Exception>(() => this.communicationService.GetUsedExhibitsAsync(caseId, cmsAuthValues));
        Assert.Equal("API Error", ex.Message);

        this.apiClientMock.Verify(client => client.GetUsedExhibitsAsync(It.Is<GetUsedExhibitsRequest>(r => r.CaseId == caseId), cmsAuthValues), Times.Once);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Fetching used exhibits for caseId [{caseIdString}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching used exhibits for caseId [{caseIdString}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains("API Error"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.GetUnusedMaterialsAsync(int, CmsAuthValues)"/>
    /// returns unused materials when the API returns valid unused materials.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetUnusedMaterialsAsync_ShouldReturn_WhenApiReturnsUnusedMaterials()
    {
        // Arrange
        int caseId = 1234;
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);

        // Exhibits
        var exhibit1 = new Exhibit(
                Id: 1,
                Title: "Exhibit One",
                OriginalFileName: "exhibit1.pdf",
                MaterialType: "1202",
                DocumentType: 2,
                Link: "http://example1.com",
                Status: "Pending",
                ReceivedDate: this.receivedDate,
                Reference: "some-reference",
                Producer: "some-producer");

        var exhibit2 = new Exhibit(
                Id: 2,
                Title: "Exhibit Two",
                OriginalFileName: "exhibit2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "Pending",
                ReceivedDate: this.receivedDate,
                Reference: "some-reference",
                Producer: "some-producer");

        var exhibits = new List<Exhibit>
        {
            exhibit1,
            exhibit2,
        };

        // MgForms
        var mgForm1 = new MgForm(
                Id: 3,
                Title: "MgForm One",
                OriginalFileName: "mgForm1.pdf",
                MaterialType: "1202",
                DocumentType: 2,
                Link: "http://example1.com",
                Status: "Pending",
                Date: this.receivedDate);

        var mgForm2 = new MgForm(
                Id: 4,
                Title: "MgForm Two",
                OriginalFileName: "mgForm2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "Pending",
                Date: this.receivedDate);

        var mgForms = new List<MgForm>
        {
            mgForm1,
            mgForm2,
        };

        // OtherMaterials
        var otherMaterial1 = new MgForm(
                Id: 5,
                Title: "MgForm One",
                OriginalFileName: "otherMaterial1.pdf",
                MaterialType: "1202",
                DocumentType: 2,
                Link: "http://example1.com",
                Status: "Pending",
                Date: this.receivedDate);

        var otherMaterial2 = new MgForm(
                Id: 6,
                Title: "MgForm Two",
                OriginalFileName: "otherMaterial2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "Pending",
                Date: this.receivedDate);

        var otherMaterials = new List<MgForm>
        {
            otherMaterial1,
            otherMaterial2,
        };

        // Statements
        var statement1 = new Statement(
                Id: 7,
                WitnessId: 789,
                Title: "Statement One",
                OriginalFileName: "statement1.pdf",
                MaterialType: "1202",
                DocumentType: 2,
                Link: "http://example1.com",
                Status: "Pending",
                ReceivedDate: this.receivedDate,
                StatementTakenDate: this.statementTakenDate);

        var statement2 = new Statement(
                Id: 8,
                WitnessId: 789,
                Title: "Statement Two",
                OriginalFileName: "statement2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "Pending",
                ReceivedDate: this.receivedDate,
                StatementTakenDate: this.statementTakenDate);

        var statements = new List<Statement>
        {
            statement1,
            statement2,
        };

        // UnusedMaterials
        var apiUnusedMaterials = new UnusedMaterialsResponse
        {
            Exhibits = exhibits,
            MgForms = mgForms,
            OtherMaterials = otherMaterials,
            Statements = statements,
        };

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        this.apiClientMock
            .Setup(client => client.GetUnusedMaterialsAsync(It.IsAny<GetUnusedMaterialsRequest>(), cmsAuthValues))
            .ReturnsAsync(apiUnusedMaterials);

        // Act
        UnusedMaterialsResponse result = await this.communicationService.GetUnusedMaterialsAsync(caseId, cmsAuthValues);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Exhibits);
        Assert.NotNull(result.MgForms);
        Assert.NotNull(result.OtherMaterials);
        Assert.NotNull(result.Statements);
        Assert.Equal(2, result.Exhibits.Count);
        Assert.Equal(2, result.MgForms.Count);
        Assert.Equal(2, result.OtherMaterials.Count);
        Assert.Equal(2, result.Statements.Count);

        Assert.Equal(1, result.Exhibits[0].Id);
        Assert.Equal(2, result.Exhibits[1].Id);
        Assert.Equal(3, result.MgForms[0].Id);
        Assert.Equal(4, result.MgForms[1].Id);
        Assert.Equal(5, result.OtherMaterials[0].Id);
        Assert.Equal(6, result.OtherMaterials[1].Id);
        Assert.Equal(7, result.Statements[0].Id);
        Assert.Equal(8, result.Statements[1].Id);

        Assert.Equal("exhibit1.pdf", result.Exhibits[0].OriginalFileName);
        Assert.Equal("exhibit2.pdf", result.Exhibits[1].OriginalFileName);
        Assert.Equal("mgForm1.pdf", result.MgForms[0].OriginalFileName);
        Assert.Equal("mgForm2.pdf", result.MgForms[1].OriginalFileName);
        Assert.Equal("otherMaterial1.pdf", result.OtherMaterials[0].OriginalFileName);
        Assert.Equal("otherMaterial2.pdf", result.OtherMaterials[1].OriginalFileName);
        Assert.Equal("statement1.pdf", result.Statements[0].OriginalFileName);
        Assert.Equal("statement2.pdf", result.Statements[1].OriginalFileName);
        Assert.Equal(this.receivedDate, result.Statements.First().ReceivedDate);
        Assert.Equal(this.statementTakenDate, result.Statements.First().StatementTakenDate);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"Fetching unused materials for caseId [{caseIdString}]"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.GetUnusedMaterialsAsync(int, CmsAuthValues)"/>
    /// logs an error and rethrows the exception when the API throws an exception.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetUnusedMaterialsAsync_ShouldLogErrorAndRethrow_WhenApiThrowsException()
    {
        // Arrange
        int caseId = 1234;
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);
        var exception = new Exception("API Error");

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        this.apiClientMock
            .Setup(client => client.GetUnusedMaterialsAsync(It.IsAny<GetUnusedMaterialsRequest>(), cmsAuthValues))
            .ThrowsAsync(exception);

        // Act & Assert
        Exception ex = await Assert.ThrowsAsync<Exception>(() => this.communicationService.GetUnusedMaterialsAsync(caseId, cmsAuthValues));
        Assert.Equal("API Error", ex.Message);

        this.apiClientMock.Verify(client => client.GetUnusedMaterialsAsync(It.Is<GetUnusedMaterialsRequest>(r => r.CaseId == caseId), cmsAuthValues), Times.Once);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Fetching unused materials for caseId [{caseIdString}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching unused materials for caseId [{caseIdString}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains("API Error"));
    }

    /// <summary>
    /// Tests that the <see cref="CommunicationService.GetAttachmentsAsync"/> method
    /// returns the attachments when the API call is successful.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetAttachmentsAsync_ShouldReturnAttachments_WhenApiCallIsSuccessful()
    {
        // Arrange
        int communicationId = 123;
        string communicationSubject = "Some Subject";

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        var expectedResponse = new AttachmentsResponse
        {
            Attachments = new List<Attachment>
            {
                new Attachment(
                    MaterialId: 1,
                    Name: "Attachment1",
                    Description: "Description1",
                    Link: "http://example1.com",
                    Classification: "Classification1",
                    DocumentTypeId: 1,
                    NumOfDocVersions: 1,
                    Statement: null,
                    Exhibit: null,
                    Tag: "Tag1",
                    DocId: 101,
                    OriginalFileName: "File1.pdf",
                    CheckedOutTo: "User1",
                    DocumentId: 1001,
                    OcrProcessed: "Yes",
                    Direction: "Inbound"),
            },
        };

        this.apiClientMock
            .Setup(api => api.GetAttachmentsAsync(It.IsAny<GetAttachmentsRequest>(), cmsAuthValues))
            .ReturnsAsync(expectedResponse);

        // Act
        AttachmentsResponse result = await this.communicationService.GetAttachmentsAsync(communicationId, communicationSubject, cmsAuthValues);

        // Assert
        Assert.Equal(expectedResponse, result);

        // Verify logging
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null &&
            log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Fetching attachments for communicationId [{communicationId}]"));
    }

    /// <summary>
    /// Tests that the <see cref="CommunicationService.GetAttachmentsAsync"/> method
    /// returns the attachments with exhibits and statments when the API call is successful.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetAttachmentsAsync_ShouldReturnAttachmentsWithExhibitsAndStatements_WhenApiCallIsSuccessful()
    {
        // Arrange
        int communicationId = 123;
        string communicationSubject = "Some Subject";

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        var expectedResponse = new AttachmentsResponse
        {
            Attachments = new List<Attachment>
            {
                new Attachment(
                    MaterialId: 1,
                    Name: "Attachment1",
                    Description: "Description1",
                    Link: "http://example1.com",
                    Classification: "Classification1",
                    DocumentTypeId: 1,
                    NumOfDocVersions: 1,
                    Statement: new StatementAttachmentSubType(
                        WitnessName: "John Doe",
                        WitnessTitle: "Detective",
                        WitnessShoulderNo: "12345",
                        StatementNo: "ST123",
                        Date: "2025-01-15",
                        Witness: 1),
                    Exhibit: new ExhibitAttachmentSubType(
                        Reference: "Exhibit123",
                        Item: "Item Description",
                        Producer: "Producer Name"),
                    Tag: "Tag1",
                    DocId: 101,
                    OriginalFileName: "File1.pdf",
                    CheckedOutTo: "User1",
                    DocumentId: 1001,
                    OcrProcessed: "Yes",
                    Direction: "Inbound"),
            },
        };

        this.apiClientMock
            .Setup(api => api.GetAttachmentsAsync(It.IsAny<GetAttachmentsRequest>(), cmsAuthValues))
            .ReturnsAsync(expectedResponse);

        // Act
        AttachmentsResponse result = await this.communicationService.GetAttachmentsAsync(communicationId, communicationSubject, cmsAuthValues);

        // Assert
        Assert.Equal(expectedResponse, result);

        // Verify logging
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null &&
            log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Fetching attachments for communicationId [{communicationId}]"));
    }

    /// <summary>
    /// Tests that the <see cref="CommunicationService.GetAttachmentsAsync"/> method
    /// logs and rethrows the exception when the API call fails.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetAttachmentsAsync_ShouldLogAndRethrowException_WhenApiCallFails()
    {
        // Arrange
        int communicationId = 123;
        string communicationSubject = "Some Subject";

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        var testException = new Exception("Simulated API error");

        this.apiClientMock
            .Setup(api => api.GetAttachmentsAsync(It.IsAny<GetAttachmentsRequest>(), cmsAuthValues))
            .ThrowsAsync(testException);

        // Act & Assert
        Exception exception = await Assert.ThrowsAsync<Exception>(() =>
            this.communicationService.GetAttachmentsAsync(communicationId, communicationSubject, cmsAuthValues));

        Assert.Equal("Simulated API error", exception.Message);

        // Verify logging
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null &&
            log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching attachments for communicationId [{communicationId}] with subject [{communicationSubject}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null &&
            log.Message.Contains("Simulated API error"));
    }

    /// <summary>
    /// Tests that the <see cref="CommunicationService.RetrieveAllAttachmentsAsync"/> method
    /// correctly retrieves and aggregates attachments for communications that have attachments.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task RetrieveAllAttachmentsAsync_ShouldRetrieveAttachments_WhenCommunicationsHaveAttachments()
    {
        // Arrange
        var communications = new List<Communication>
        {
            new Communication(
                Id: 1,
                OriginalFileName: "File1",
                Subject: "Subject1",
                DocumentTypeId: 123,
                MaterialId: 456,
                Link: "http://example.com",
                Status: "Active",
                Category: "Category1",
                Type: "Type1",
                HasAttachments: true),

            new Communication(
                Id: 2,
                OriginalFileName: "File2",
                Subject: "Subject2",
                DocumentTypeId: 789,
                MaterialId: 1011,
                Link: "http://example.org",
                Status: "Inactive",
                Category: "Category2",
                Type: "Type2",
                HasAttachments: true),
        };

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        var attachmentsResponse = new AttachmentsResponse
        {
            Attachments = new List<Attachment>
            {
                new Attachment(
                    MaterialId: 1,
                    Name: "Attachment1",
                    Description: "Description",
                    Link: "http://example1.com",
                    Classification: "Classification",
                    DocumentTypeId: 123,
                    NumOfDocVersions: 1,
                    Statement: null,
                    Exhibit: null,
                    Tag: "Tag",
                    DocId: 101,
                    OriginalFileName: "TestFile1.pdf",
                    CheckedOutTo: "CheckedOutTo",
                    DocumentId: 1001,
                    OcrProcessed: "OCRProcessed",
                    Direction: "Direction"),

                new Attachment(
                    MaterialId: 2,
                    Name: "Attachment2",
                    Description: "Description",
                    Link: "http://example2.com",
                    Classification: "Classification",
                    DocumentTypeId: 123,
                    NumOfDocVersions: 1,
                    Statement: null,
                    Exhibit: null,
                    Tag: "Tag",
                    DocId: 101,
                    OriginalFileName: "TestFile2.pdf",
                    CheckedOutTo: "CheckedOutTo",
                    DocumentId: 1001,
                    OcrProcessed: "OCRProcessed",
                    Direction: "Direction"),
            },
        };

        this.apiClientMock
            .Setup(s => s.GetAttachmentsAsync(It.IsAny<GetAttachmentsRequest>(), cmsAuthValues))
            .ReturnsAsync(attachmentsResponse);

        // Act
        List<Attachment> result = await this.communicationService.RetrieveAllAttachmentsAsync(communications, cmsAuthValues);

        // Assert
        Assert.Equal(4, result.Count);
        Assert.Contains(result, a => a.Name == "Attachment1");
        Assert.Contains(result, a => a.Name == "Attachment2");

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Communication Id [1] with attachments retrieved, subject [Subject1]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Communication Id [2] with attachments retrieved, subject [Subject2]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Total attachments retrieved: 4"));
    }

    /// <summary>
    /// Tests that the <see cref="CommunicationService.RetrieveAllAttachmentsAsync"/> method
    /// correctly retrieves and aggregates attachments with exhibits and statements for communications that have attachments.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task RetrieveAllAttachmentsAsync_ShouldRetrieveAttachmentsWithExhibitsAndStatements_WhenCommunicationsHaveAttachments()
    {
        // Arrange
        var communications = new List<Communication>
        {
            new Communication(
                Id: 1,
                OriginalFileName: "File1",
                Subject: "Subject1",
                DocumentTypeId: 123,
                MaterialId: 456,
                Link: "http://example.com",
                Status: "Active",
                Category: "Category1",
                Type: "Type1",
                HasAttachments: true),

            new Communication(
                Id: 2,
                OriginalFileName: "File2",
                Subject: "Subject2",
                DocumentTypeId: 789,
                MaterialId: 1011,
                Link: "http://example.org",
                Status: "Inactive",
                Category: "Category2",
                Type: "Type2",
                HasAttachments: true),
        };

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        var attachmentsResponse = new AttachmentsResponse
        {
            Attachments = new List<Attachment>
            {
                new Attachment(
                    MaterialId: 1,
                    Name: "Attachment1",
                    Description: "Description",
                    Link: "http://example1.com",
                    Classification: "Classification",
                    DocumentTypeId: 123,
                    NumOfDocVersions: 1,
                    Statement: new StatementAttachmentSubType(
                        WitnessName: "John Doe",
                        WitnessTitle: "Detective",
                        WitnessShoulderNo: "12345",
                        StatementNo: "ST123",
                        Date: "2025-01-15",
                        Witness: 1),
                    Exhibit: new ExhibitAttachmentSubType(
                        Reference: "Exhibit123",
                        Item: "Item Description",
                        Producer: "Producer Name"),
                    Tag: "Tag",
                    DocId: 101,
                    OriginalFileName: "TestFile1.pdf",
                    CheckedOutTo: "CheckedOutTo",
                    DocumentId: 1001,
                    OcrProcessed: "OCRProcessed",
                    Direction: "Direction"),

                new Attachment(
                    MaterialId: 2,
                    Name: "Attachment2",
                    Description: "Description",
                    Link: "http://example2.com",
                    Classification: "Classification",
                    DocumentTypeId: 123,
                    NumOfDocVersions: 1,
                    Statement: new StatementAttachmentSubType(
                        WitnessName: "Jim Bob",
                        WitnessTitle: "Detective",
                        WitnessShoulderNo: "12345",
                        StatementNo: "ST123",
                        Date: "2025-01-15",
                        Witness: 1),
                    Exhibit: new ExhibitAttachmentSubType(
                        Reference: "Exhibit456",
                        Item: "Item Description",
                        Producer: "Producer Name"),
                    Tag: "Tag",
                    DocId: 101,
                    OriginalFileName: "TestFile2.pdf",
                    CheckedOutTo: "CheckedOutTo",
                    DocumentId: 1001,
                    OcrProcessed: "OCRProcessed",
                    Direction: "Direction"),
            },
        };

        this.apiClientMock
            .Setup(s => s.GetAttachmentsAsync(It.IsAny<GetAttachmentsRequest>(), cmsAuthValues))
            .ReturnsAsync(attachmentsResponse);

        // Act
        List<Attachment> result = await this.communicationService.RetrieveAllAttachmentsAsync(communications, cmsAuthValues);

        // Assert
        Assert.Equal(4, result.Count);
        Assert.Contains(result, a => a.Name == "Attachment1");
        Assert.Contains(result, a => a.Name == "Attachment2");

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Communication Id [1] with attachments retrieved, subject [Subject1]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Communication Id [2] with attachments retrieved, subject [Subject2]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Total attachments retrieved: 4"));
    }

    /// <summary>
    /// Tests that the <see cref="CommunicationService.RetrieveAllAttachmentsAsync"/> method
    /// returns an empty list when no communications are provided.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task RetrieveAllAttachmentsAsync_ShouldReturnEmptyList_WhenNoCommunications()
    {
        // Arrange
        var communications = new List<Communication>();

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        // Act
        List<Attachment> result = await this.communicationService.RetrieveAllAttachmentsAsync(communications, cmsAuthValues);

        // Assert
        Assert.Empty(result);

        // Verify no calls were made to the GetAttachmentsAsync method
        this.apiClientMock.Verify(
            s => s.GetAttachmentsAsync(It.IsAny<GetAttachmentsRequest>(), It.IsAny<CmsAuthValues>()),
            Times.Never);

        // Verify a log was generated for total attachments retrieved
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Total attachments retrieved: 0"));
    }

    /// <summary>
    /// Tests that the <see cref="CommunicationService.RetrieveAllAttachmentsAsync"/> method
    /// correctly handles communications that do not have attachments.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task RetrieveAllAttachmentsAsync_ShouldHandleCommunicationsWithoutAttachments()
    {
        // Arrange
        var communications = new List<Communication>
        {
            new Communication(
                Id: 1,
                OriginalFileName: "File1",
                Subject: "Subject1",
                DocumentTypeId: 123,
                MaterialId: 456,
                Link: "http://example.com",
                Status: "Active",
                Category: "Category1",
                Type: "Type1",
                HasAttachments: false), // No attachments
        };

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        // Act
        List<Attachment> result = await this.communicationService.RetrieveAllAttachmentsAsync(communications, cmsAuthValues);

        // Assert
        Assert.Empty(result);

        // Verify no calls were made to the GetAttachmentsAsync method
        this.apiClientMock.Verify(
            s => s.GetAttachmentsAsync(It.IsAny<GetAttachmentsRequest>(), It.IsAny<CmsAuthValues>()),
            Times.Never);

        // Verify a log was generated for total attachments retrieved
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Total attachments retrieved: 0"));
    }

    /// <summary>
    /// Tests that the <see cref="CommunicationService.RetrieveAllAttachmentsAsync"/> method
    /// logs and rethrows exceptions when an error occurs during execution.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task RetrieveAllAttachmentsAsync_ShouldLogAndRethrowException_WhenErrorOccurs()
    {
        // Arrange
        var communications = new List<Communication>
        {
            new Communication(
                Id: 1,
                OriginalFileName: "File1",
                Subject: "Subject1",
                DocumentTypeId: 123,
                MaterialId: 456,
                Link: "http://example.com",
                Status: "Active",
                Category: "Category1",
                Type: "Type1",
                HasAttachments: true),
        };

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        var testException = new Exception("Simulated error");

        // Simulate an error when calling GetAttachmentsAsync
        this.apiClientMock
            .Setup(s => s.GetAttachmentsAsync(It.IsAny<GetAttachmentsRequest>(), cmsAuthValues))
            .ThrowsAsync(testException);

        // Act & Assert
        Exception exception = await Assert.ThrowsAsync<Exception>(() =>
            this.communicationService.RetrieveAllAttachmentsAsync(communications, cmsAuthValues));

        Assert.Equal("Simulated error", exception.Message); // The exception should match the simulated error

        // Verify that the exception was logged
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Error occurred while trying to retrieve all attachments"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains("Simulated error"));
    }

    /// <summary>
    /// Unit test for MapAttachmentsToCommunications to verify correct mapping from Attachment to Communication.
    /// </summary>
    [Fact]
    public void MapAttachmentsToCommunications_ShouldMapCorrectly()
    {
        // Arrange
        var attachments = new List<Attachment>
        {
            new Attachment(
                MaterialId: 1,
                Name: "Test Attachment",
                Description: "Description",
                Link: "http://example.com",
                Classification: "Classification",
                DocumentTypeId: 123,
                NumOfDocVersions: 1,
                Statement: null,
                Exhibit: null,
                Tag: "Tag",
                DocId: 101,
                OriginalFileName: "TestFile.pdf",
                CheckedOutTo: "CheckedOutTo",
                DocumentId: 1001,
                OcrProcessed: "OCRProcessed",
                Direction: "Direction"),
        };

        var documentTypeInfo = new DocumentTypeInfo
        {
            Category = "Category A",
            DocumentType = "Type X",
        };

        this.documentTypeMapperMock
            .Setup(m => m.MapDocumentType(It.IsAny<int>())).Returns(documentTypeInfo);

        // Act
        List<Communication> result = this.communicationService.MapAttachmentsToCommunications(attachments);

        // Assert
        Communication communication = result.First();
        Assert.Equal(1, communication.MaterialId);
        Assert.Equal("Test Attachment", communication.Subject);
        Assert.Equal("TestFile.pdf", communication.OriginalFileName);
        Assert.Equal(123, communication.DocumentTypeId);
        Assert.Equal("http://example.com", communication.Link);
        Assert.Equal("Category A", communication.Category);
        Assert.Equal("Type X", communication.Type);
        Assert.False(communication.HasAttachments);
    }

    /// <summary>
    /// Unit test for MapAttachmentsToCommunications to verify correct mapping from Attachment to Communication.
    /// </summary>
    [Fact]
    public void MapAttachmentsWithExhibitsAndStatementsToCommunications_ShouldMapCorrectly()
    {
        // Arrange
        var attachments = new List<Attachment>
        {
            new Attachment(
                MaterialId: 1,
                Name: "Test Attachment",
                Description: "Description",
                Link: "http://example.com",
                Classification: "Classification",
                DocumentTypeId: 123,
                NumOfDocVersions: 1,
                Statement: new StatementAttachmentSubType(
                        WitnessName: "John Doe",
                        WitnessTitle: "Detective",
                        WitnessShoulderNo: "12345",
                        StatementNo: "ST123",
                        Date: "2025-01-15",
                        Witness: 1),
                Exhibit: new ExhibitAttachmentSubType(
                        Reference: "Exhibit123",
                        Item: "Item Description",
                        Producer: "Producer Name"),
                Tag: "Tag",
                DocId: 101,
                OriginalFileName: "TestFile.pdf",
                CheckedOutTo: "CheckedOutTo",
                DocumentId: 1001,
                OcrProcessed: "OCRProcessed",
                Direction: "Direction"),
        };

        var documentTypeInfo = new DocumentTypeInfo
        {
            Category = "Category A",
            DocumentType = "Type X",
        };

        this.documentTypeMapperMock
            .Setup(m => m.MapDocumentType(It.IsAny<int>())).Returns(documentTypeInfo);

        // Act
        List<Communication> result = this.communicationService.MapAttachmentsToCommunications(attachments);

        // Assert
        Communication communication = result.First();
        Assert.Equal(1, communication.MaterialId);
        Assert.Equal("Test Attachment", communication.Subject);
        Assert.Equal("TestFile.pdf", communication.OriginalFileName);
        Assert.Equal(123, communication.DocumentTypeId);
        Assert.Equal("http://example.com", communication.Link);
        Assert.Equal("Category A", communication.Category);
        Assert.Equal("Type X", communication.Type);
        Assert.False(communication.HasAttachments);
    }

    /// <summary>
    /// Tests that the MapAttachmentsToCommunications method correctly assigns default values
    /// when properties of an attachment are null.
    /// </summary>
    [Fact]
    public void MapAttachmentsToCommunications_ShouldUseDefaultValues_WhenPropertiesAreNull()
    {
        // Arrange
        var attachments = new List<Attachment>
        {
            new Attachment(
                MaterialId: 1,
                Name: null,
                Description: "Description",
                Link: null,
                Classification: "Classification",
                DocumentTypeId: 123,
                NumOfDocVersions: 1,
                Statement: null,
                Exhibit: null,
                Tag: null,
                DocId: 101,
                OriginalFileName: null,
                CheckedOutTo: "CheckedOutTo",
                DocumentId: 1001,
                OcrProcessed: "OCRProcessed",
                Direction: "Direction"),
        };

        var documentTypeInfo = new DocumentTypeInfo
        {
            Category = "Category A",
            DocumentType = "Type X",
        };

        this.documentTypeMapperMock
            .Setup(m => m.MapDocumentType(It.IsAny<int>())).Returns(documentTypeInfo);

        // Act
        List<Communication> result = this.communicationService.MapAttachmentsToCommunications(attachments);

        // Assert
        Communication communication = result.First();
        Assert.Equal("Unknown File Name", communication.OriginalFileName);
        Assert.Equal("No Subject", communication.Subject);
        Assert.Equal(string.Empty, communication.Link);
    }

    /// <summary>
    /// Tests that the MapAttachmentsToCommunications method returns an empty list
    /// when there are no attachments provided.
    /// </summary>
    [Fact]
    public void MapAttachmentsToCommunications_ShouldReturnEmptyList_WhenNoAttachments()
    {
        // Arrange
        var attachments = new List<Attachment>();

        // Act
        List<Communication> result = this.communicationService.MapAttachmentsToCommunications(attachments);

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Tests that the MapAttachmentsToCommunications method logs an error
    /// and rethrows the exception when an exception is thrown during execution.
    /// </summary>
    [Fact]
    public void MapAttachmentsToCommunications_ShouldLogError_WhenExceptionIsThrown()
    {
        // Arrange
        var attachments = new List<Attachment>
        {
            new Attachment(
                MaterialId: 1,
                Name: "Test Attachment",
                Description: "Description",
                Link: "http://example.com",
                Classification: "Classification",
                DocumentTypeId: 123,
                NumOfDocVersions: 1,
                Statement: null,
                Exhibit: null,
                Tag: "Tag",
                DocId: 101,
                OriginalFileName: "TestFile.pdf",
                CheckedOutTo: "CheckedOutTo",
                DocumentId: 1001,
                OcrProcessed: "OCRProcessed",
                Direction: "Direction"),
        };

        // Simulate an error in the documentTypeMapper
        this.documentTypeMapperMock
            .Setup(m => m.MapDocumentType(It.IsAny<int>())).Throws(new Exception("Test Exception"));

        // Act & Assert
        Exception exception = Assert.Throws<Exception>(() => this.communicationService.MapAttachmentsToCommunications(attachments));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains("Test Exception"));
    }

    /// <summary>
    /// Tests that GetCommunicationLink returns the correct link when a matching communication is found.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetCommunicationLinkAsync_CommunicationLinkFound_ReturnsLink()
    {
        // Arrange
        int caseId = 1;
        int materialId = 456;

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        var apiCommunications = new List<Communication>
        {
            new Communication(
            Id: 1,
            OriginalFileName: "File1",
            Subject: "Subject1",
            DocumentTypeId: 123,
            MaterialId: 456,
            Link: "/some/path/doc1.pdf",
            Status: "Active",
            Category: "Category1",
            Type: "Type1",
            HasAttachments: false),
        };

        var documentTypeInfo = new DocumentTypeInfo
        {
            Category = "Category A",
            DocumentType = "Type X",
        };

        this.documentTypeMapperMock
            .Setup(m => m.MapDocumentType(It.IsAny<int>())).Returns(documentTypeInfo);

        this.apiClientMock
            .Setup(client => client.ListCommunicationsHkAsync(It.IsAny<ListCommunicationsHkRequest>(), cmsAuthValues))
            .ReturnsAsync(apiCommunications);

        // Act
        object result = await this.communicationService.GetCaseMaterialLinkAsync(caseId, materialId, cmsAuthValues);

        // Assert
        Assert.Equal("/some/path/doc1.pdf", result);
    }

    /// <summary>
    /// Tests whether <see cref="CommunicationService.GetCaseMaterialLinkAsync"/> correctly retrieves the expected material link
    /// from various sources, including communications, used statements, used exhibits, and unused materials.
    /// </summary>
    /// <param name="source">The source from which the material is expected to be found.</param>
    /// <param name="expectedLink">The expected link to the material.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Theory]
    [InlineData("communications", "/some/path/doc1.pdf")] // Found in communications
    [InlineData("usedStatements", "/some/path/statement.pdf")] // Found in usedStatements
    [InlineData("usedExhibits", "/some/path/exhibit.pdf")] // Found in usedExhibits
    [InlineData("unusedMaterials.Exhibits", "/some/path/unused-exhibit.pdf")] // Found in unusedMaterials.Exhibits
    [InlineData("unusedMaterials.MgForms", "/some/path/mg-form.pdf")] // Found in unusedMaterials.MgForms
    [InlineData("unusedMaterials.OtherMaterials", "/some/path/other-material.pdf")] // Found in unusedMaterials.OtherMaterials
    [InlineData("unusedMaterials.Statements", "/some/path/unused-statement.pdf")] // Found in unusedMaterials.Statements
    [InlineData("usedMgForms.MgForms", "/some/path/used-mg-form.pdf")] // Found in usedMgForms.MgForms
    [InlineData("usedOtherMaterials.MgForms", "/some/path/used-other-materials.pdf")] // Found in usedOtherMaterials.MgForms
    public async Task GetCaseMaterialLinkAsync_MaterialFoundInVariousSources_ReturnsCorrectLink(string source, string expectedLink)
    {
        // Arrange
        int caseId = 1;
        int materialId = 456;
        var cmsAuthValues = new CmsAuthValues("cookies", "token", Guid.NewGuid());

        List<Communication> communications = source == "communications"
            ? new List<Communication> { new(1, "File1", "Subject1", 123, materialId, expectedLink, "Active", "Category1", "Type1", false) }
            : new List<Communication>();

        var usedStatements = new UsedStatementsResponse
        {
            Statements = source == "usedStatements"
                ? new List<Statement> { new Statement(materialId, 789, "Title", "Description", "SomeType", 789, expectedLink, "Category", this.receivedDate, this.statementTakenDate) }
                : new List<Statement>(),
        };

        var usedExhibits = new UsedExhibitsResponse
        {
            Exhibits = source == "usedExhibits"
                ? new List<Exhibit> { new Exhibit(materialId, "ExhibitTitle", "ExhibitFile.pdf", "MaterialType", 567, expectedLink, "Active", this.receivedDate, "some-reference", "some-producer") }
                : new List<Exhibit>(),
        };

        var usedMgForms = new UsedMgFormsResponse
        {
            MgForms = source == "usedMgForms.MgForms"
                ? new List<MgForm> { new MgForm(materialId, "Used MgForm Title", "UsedMgFormFile.pdf", "MaterialType", 898, expectedLink, "Active", this.receivedDate) }
                : new List<MgForm>(),
        };

        var usedOtherMaterials = new UsedOtherMaterialsResponse
        {
            MgForms = source == "usedOtherMaterials.MgForms"
                ? new List<MgForm> { new MgForm(materialId, "Used Other Material Title", "UsedOtherMaterialFile.pdf", "MaterialType", 989, expectedLink, "Active", this.receivedDate) }
                : new List<MgForm>(),
        };

        var unusedMaterials = new UnusedMaterialsResponse
        {
            Exhibits = source == "unusedMaterials.Exhibits"
                ? new List<Exhibit> { new Exhibit(materialId, "Exhibit Title", "ExhibitFile.pdf", "MaterialType", 123, expectedLink, "Active", this.receivedDate, "some-reference", "some-producer") }
                : new List<Exhibit>(),

            MgForms = source == "unusedMaterials.MgForms"
                ? new List<MgForm> { new MgForm(materialId, "MgForm Title", "MgFormFile.pdf", "MaterialType", 456, expectedLink, "Active", this.receivedDate) }
                : new List<MgForm>(),

            OtherMaterials = source == "unusedMaterials.OtherMaterials"
                ? new List<MgForm> { new MgForm(materialId, "Other Material Title", "OtherMaterialFile.pdf", "MaterialType", 789, expectedLink, "Active", this.receivedDate) }
                : new List<MgForm>(),

            Statements = source == "unusedMaterials.Statements"
                ? new List<Statement> { new Statement(materialId, 789, "Statement Title", "StatementFile.pdf", "MaterialType", 353, expectedLink, "Active", this.receivedDate, this.statementTakenDate) }
                : new List<Statement>(),
        };

        var documentTypeInfo = new DocumentTypeInfo
        {
            Category = "Category A",
            DocumentType = "Type X",
        };

        this.documentTypeMapperMock
            .Setup(m => m.MapDocumentType(It.IsAny<int>())).Returns(documentTypeInfo);

        this.apiClientMock
            .Setup(client => client.ListCommunicationsHkAsync(It.IsAny<ListCommunicationsHkRequest>(), cmsAuthValues))
            .ReturnsAsync(communications);

        var unusedMaterialsRequest = new GetUnusedMaterialsRequest(caseId, Guid.NewGuid());
        this.apiClientMock
            .Setup(client => client.GetUnusedMaterialsAsync(It.Is<GetUnusedMaterialsRequest>(r => r.CaseId == caseId), cmsAuthValues))
            .ReturnsAsync(unusedMaterials);

        var usedStatementsRequest = new GetUsedStatementsRequest(caseId, Guid.NewGuid());
        this.apiClientMock
            .Setup(client => client.GetUsedStatementsAsync(It.Is<GetUsedStatementsRequest>(r => r.CaseId == caseId), cmsAuthValues))
            .ReturnsAsync(usedStatements);

        var usedExhibitsRequest = new GetUsedExhibitsRequest(caseId, Guid.NewGuid());
        this.apiClientMock
            .Setup(client => client.GetUsedExhibitsAsync(It.Is<GetUsedExhibitsRequest>(r => r.CaseId == caseId), cmsAuthValues))
            .ReturnsAsync(usedExhibits);

        var usedMgFormsRequest = new GetUsedMgFormsRequest(caseId, Guid.NewGuid());
        this.apiClientMock
            .Setup(client => client.GetUsedMgFormsAsync(It.Is<GetUsedMgFormsRequest>(r => r.CaseId == caseId), cmsAuthValues))
            .ReturnsAsync(usedMgForms);

        var usedOtherMaterialsRequest = new GetUsedOtherMaterialsRequest(caseId, Guid.NewGuid());
        this.apiClientMock
            .Setup(client => client.GetUsedOtherMaterialsAsync(It.Is<GetUsedOtherMaterialsRequest>(r => r.CaseId == caseId), cmsAuthValues))
            .ReturnsAsync(usedOtherMaterials);

        // Act
        object result = await this.communicationService.GetCaseMaterialLinkAsync(caseId, materialId, cmsAuthValues);

        // Assert
        Assert.Equal(expectedLink, result);
    }

    /// <summary>
    /// Verifies that the method returns a NotFoundObjectResult when no communications are available.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetCommunicationLinkAsync_NoCommunications_ReturnsNotFoundResult()
    {
        // Arrange
        int caseId = 1;
        int materialId = 456;

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        this.apiClientMock
            .Setup(client => client.ListCommunicationsHkAsync(It.IsAny<ListCommunicationsHkRequest>(), cmsAuthValues))
            .ReturnsAsync(new List<Communication>());

        // Act
        object result = await this.communicationService.GetCaseMaterialLinkAsync(caseId, materialId, cmsAuthValues);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        string? value = ((NotFoundObjectResult)result).Value?.ToString();
        Assert.Contains("No case material document found for materialId", value ?? string.Empty);
    }

    /// <summary>
    /// Verifies that the method searches for a link in attachments if no direct communication is found.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetCommunicationLinkAsync_NoDirectLink_ChecksAttachments_ReturnsLink()
    {
        // Arrange
        int caseId = 1;
        int materialId = 789;

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        var apiCommunications = new List<Communication>
        {
            new (
            Id: 1,
            OriginalFileName: "File1",
            Subject: "Subject1",
            DocumentTypeId: 123,
            MaterialId: 456,
            Link: "/some/path/doc1.pdf",
            Status: "Active",
            Category: "Category1",
            Type: "Type1",
            HasAttachments: true),
        };

        var documentTypeInfo = new DocumentTypeInfo
        {
            Category = "Category A",
            DocumentType = "Type X",
        };

        this.documentTypeMapperMock
            .Setup(m => m.MapDocumentType(It.IsAny<int>())).Returns(documentTypeInfo);

        var expectedResponse = new AttachmentsResponse
        {
            Attachments =
            [
                new (
                    MaterialId: 789,
                    Name: "Attachment1",
                    Description: "Description1",
                    Link: "/some/path/attachment.pdf",
                    Classification: "Classification1",
                    DocumentTypeId: 1,
                    NumOfDocVersions: 1,
                    Statement: null,
                    Exhibit: null,
                    Tag: "Tag1",
                    DocId: 101,
                    OriginalFileName: "File1.pdf",
                    CheckedOutTo: "User1",
                    DocumentId: 1001,
                    OcrProcessed: "Yes",
                    Direction: "Inbound"),
            ],
        };

        this.apiClientMock
            .Setup(client => client.ListCommunicationsHkAsync(It.IsAny<ListCommunicationsHkRequest>(), cmsAuthValues))
            .ReturnsAsync(apiCommunications);

        this.apiClientMock
            .Setup(api => api.GetAttachmentsAsync(It.IsAny<GetAttachmentsRequest>(), cmsAuthValues))
            .ReturnsAsync(expectedResponse);

        // Act
        object result = await this.communicationService.GetCaseMaterialLinkAsync(caseId, materialId, cmsAuthValues);

        // Assert
        Assert.Equal("/some/path/attachment.pdf", result);
    }

    /// <summary>
    /// Verifies that the method returns NotFoundObjectResult when no attachments are found.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetCommunicationLinkAsync_NoAttachments_ReturnsNotFoundResult()
    {
        // Arrange
        int caseId = 1;
        int materialId = 789;

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        var apiCommunications = new List<Communication>
        {
            new (
            Id: 1,
            OriginalFileName: "File1",
            Subject: "Subject1",
            DocumentTypeId: 123,
            MaterialId: 456,
            Link: "/some/path/doc1.pdf",
            Status: "Active",
            Category: "Category1",
            Type: "Type1",
            HasAttachments: true),
        };

        var documentTypeInfo = new DocumentTypeInfo
        {
            Category = "Category A",
            DocumentType = "Type X",
        };

        this.documentTypeMapperMock
            .Setup(m => m.MapDocumentType(It.IsAny<int>())).Returns(documentTypeInfo);

        this.apiClientMock
            .Setup(client => client.ListCommunicationsHkAsync(It.IsAny<ListCommunicationsHkRequest>(), cmsAuthValues))
            .ReturnsAsync(apiCommunications);

        this.apiClientMock
            .Setup(api => api.GetAttachmentsAsync(It.IsAny<GetAttachmentsRequest>(), cmsAuthValues))
            .ReturnsAsync(new AttachmentsResponse
            {
                Attachments = new List<Attachment>(),
            });

        // Act
        object result = await this.communicationService.GetCaseMaterialLinkAsync(caseId, materialId, cmsAuthValues);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
        string? value = ((NotFoundObjectResult)result).Value?.ToString();
        Assert.Contains("No case material document found for materialId", value ?? string.Empty);
    }

    /// <summary>
    /// Verifies that the method throws an exception when an unexpected error occurs.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetCommunicationLinkAsync_ThrowsException_ErrorIsLogged()
    {
        // Arrange
        int caseId = 1;
        int materialId = 456;

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        this.apiClientMock
            .Setup(client => client.ListCommunicationsHkAsync(It.IsAny<ListCommunicationsHkRequest>(), cmsAuthValues))
            .ThrowsAsync(new Exception("API failure"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () =>
            await this.communicationService.GetCaseMaterialLinkAsync(caseId, materialId, cmsAuthValues).ConfigureAwait(false));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Error occurred while trying to get the link for materialId [456]"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.RenameMaterialAsync(int, int, string, CmsAuthValues, Guid)"/>
    /// returns expected response after renaming material.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task RenameMaterialAsync_ShouldReturn_AsExpected_SuccessLogged()
    {
        // Arrange
        var cmsAuthValues = new CmsAuthValues(
           "cookies",
           "token",
           Guid.NewGuid());

        var expectedResponse = new RenameMaterialResponse(new RenameMaterialData
        {
            Id = 1212,
        });

        int caseId = 134;
        var request = new RenameMaterialRequest(Guid.NewGuid(), 1212, "mocked file name");

        this.apiClientMock
          .Setup(client => client.RenameMaterialAsync(request, cmsAuthValues))
          .ReturnsAsync(expectedResponse);

        // Act
        RenameMaterialResponse result = await this.communicationService.RenameMaterialAsync(caseId, expectedResponse.RenameMaterialData.Id, request.subject, cmsAuthValues, request.id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);

        Assert.Contains(this.mockLogger.Logs, log =>
          log.LogLevel == LogLevel.Information &&
          log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Attempting to rename material with materialId [{expectedResponse.RenameMaterialData.Id}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] successfully renamed material name with materialId [{expectedResponse.RenameMaterialData.Id}]"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.RenameMaterialAsync(int, int, string, CmsAuthValues, Guid)"/>
    /// returns expected exception when DDEO throws exception on renaming material endpoint.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task RenameMaterialAsync_WhenApiThrow_ShouldThrowException()
    {
        // Arrange
        var cmsAuthValues = new CmsAuthValues(
           "cookies",
           "token",
           Guid.NewGuid());

        int caseId = 134;

        var exceptionMocked = new BadHttpRequestException("Material does not exists in DDEI");

        var request = new RenameMaterialRequest(Guid.NewGuid(), 1212, "mocked file name");

        this.apiClientMock
          .Setup(client => client.RenameMaterialAsync(request, cmsAuthValues))
          .ThrowsAsync(exceptionMocked);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await this.communicationService.RenameMaterialAsync(caseId, request.materialId, request.subject, cmsAuthValues, request.id).ConfigureAwait(false));

        Assert.Contains(this.mockLogger.Logs, log =>
          log.LogLevel == LogLevel.Information &&
          log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Attempting to rename material with materialId [1212]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] Failed to rename material with materialId [1212]"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.GetPcdRequestCore(int, CmsAuthValues)"/>
    /// returns expected exception with DDEI API throw an exception.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetPcdRequestCore_ShouldThrowException_WhenApiClientThrowsException()
    {
        // Arrange
        int caseId = 123;
        var correlationId = Guid.NewGuid();
        var cmsAuthValues = new CmsAuthValues(
          "cookies",
          "token",
          correlationId);
        var request = new GetPcdRequestsCoreRequest(caseId, correlationId);
        this.apiClientMock.Setup(x => x.GetPcdRequestCoreAsync(request, cmsAuthValues))
            .ThrowsAsync(new Exception("API Error"));

        // Act
        Func<Task> act = async () => await this.communicationService.GetPcdRequestCore(caseId, cmsAuthValues).ConfigureAwait(false);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("API Error");
        Assert.Contains(this.mockLogger.Logs, log =>
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching unused materials for caseId [{caseId}]"));
    }


    /// <summary>
    /// Tests that <see cref="CommunicationService.GetPcdRequestByPcdIdAsync(int, int, CmsAuthValues)"/>
    /// returns expected exception when DDEI API throw exception.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetPcdRequestByPcdIdAsync_ShouldThrowException_WhenApiClientThrowsException()
    {
        // Arrange
        int caseId = 123;
        int pcdId = 456;
        var correlationId = Guid.NewGuid();
        var cmsAuthValues = new CmsAuthValues(
        "cookies",
        "token",
        correlationId);
        var request = new GetPcdRequestByPcdIdCoreRequest(caseId, pcdId, correlationId);
        this.apiClientMock.Setup(x => x.GetPcdRequestByPcdIdAsync(request, cmsAuthValues))
            .ThrowsAsync(new Exception("API Error"));

        // Act
        Func<Task> act = async () => await this.communicationService.GetPcdRequestByPcdIdAsync(caseId, pcdId, cmsAuthValues).ConfigureAwait(false);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("API Error");
        Assert.Contains(this.mockLogger.Logs, log =>
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching PCD Request overview for caseId [{caseId}] and PCD id [{pcdId}]"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.SetMaterialReadStatusAsync(int, Interfaces.Enums.SetMaterialReadStatusType, CmsAuthValues, Guid)"/>
    /// returns expected response after renaming material.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SetMaterialReadStatusAsync_ShouldReturn_AsExpected_SuccessLogged()
    {
        // Arrange
        var cmsAuthValues = new CmsAuthValues(
           "cookies",
           "token",
           Guid.NewGuid());

        var expectedResposne = new SetMaterialReadStatusResponse(new SetMaterialReadStatusResponseData
        {
            Id = 1212,
        });

        var request = new SetMaterialReadStatusRequest(Guid.NewGuid(), 1212, MaterialReadStatusType.Invalid);

        this.apiClientMock
          .Setup(client => client.SetMaterialReadStatusAsync(request, cmsAuthValues))
          .ReturnsAsync(expectedResposne);

        // Act
        SetMaterialReadStatusResponse result = await this.communicationService.SetMaterialReadStatusAsync(request.materialId, request.state, cmsAuthValues, request.CorrespondenceId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResposne);

        Assert.Contains(this.mockLogger.Logs, log =>
          log.LogLevel == LogLevel.Information &&
          log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Attempting to mark read/unread state of material with material id [1212]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Successfully marked material read/unread state of material with material id [{expectedResposne.CompleteCommunicationData.Id}]"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.SetMaterialReadStatusAsync(int, Interfaces.Enums.SetMaterialReadStatusType, CmsAuthValues, Guid)"/>
    /// returns expected exception when DDEO throws exception on renaming material endpoint.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task SetMaterialReadStatusAsync_WhenApiThrow_ShouldThrowException()
    {
        // Arrange
        var cmsAuthValues = new CmsAuthValues(
           "cookies",
           "token",
           Guid.NewGuid());

        var exceptionMocked = new BadHttpRequestException("Material does not exists in DDEI");

        var request = new SetMaterialReadStatusRequest(Guid.NewGuid(), 1212, MaterialReadStatusType.Invalid);

        this.apiClientMock
          .Setup(client => client.SetMaterialReadStatusAsync(request, cmsAuthValues))
          .ThrowsAsync(exceptionMocked);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await this.communicationService.SetMaterialReadStatusAsync(request.materialId, request.state, cmsAuthValues, request.CorrespondenceId).ConfigureAwait(false));

        Assert.Contains(this.mockLogger.Logs, log =>
          log.LogLevel == LogLevel.Information &&
          log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Attempting to mark read/unread state of material with material id [1212]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Error occurred while renaming material with material id [1212]"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.DiscardMaterialAsync(int, int, string, string, CmsAuthValues, Guid)"/>
    /// returns expected response after discarding a material.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DiscardMaterialAsync_ShouldLogAndReturn_WhenApiReturnsSuccess()
    {
        // Arrange
        var cmsAuthValues = new CmsAuthValues(
           "cookies",
           "token",
           Guid.NewGuid());

        var expectedResponse = new DiscardMaterialResponse(new DiscardMaterialData
        {
            Id = 1212,
        });

        int caseId = 134;
        var request = new DiscardMaterialRequest(Guid.NewGuid(), 1212, "mocked discard reason", "mocked discard reason description");

        this.apiClientMock
          .Setup(client => client.DiscardMaterialAsync(request, cmsAuthValues))
          .ReturnsAsync(expectedResponse);

        // Act
        DiscardMaterialResponse result = await this.communicationService.DiscardMaterialAsync(
            caseId,
            expectedResponse.DiscardMaterialData!.Id,
            request.discardReason,
            request.discardReasonDescription,
            cmsAuthValues,
            request.id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);

        Assert.Contains(this.mockLogger.Logs, log =>
          log.LogLevel == LogLevel.Information &&
          log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Attempting to discard material with materialId [{expectedResponse.DiscardMaterialData.Id}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] successfully discarded material with materialId [{expectedResponse.DiscardMaterialData.Id}]"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.DiscardMaterialAsync(int, int, string, string, CmsAuthValues, Guid)"/>
    /// logs and rethrows the exception when the API call fails.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task DiscardMaterialAsync_ShouldLogAndRethrowException_WhenApiCallFails()
    {
        // Arrange
        var cmsAuthValues = new CmsAuthValues(
           "cookies",
           "token",
           Guid.NewGuid());

        int caseId = 134;

        var exceptionMocked = new BadHttpRequestException("Material does not exists in DDEI");

        var request = new DiscardMaterialRequest(Guid.NewGuid(), 1212, "mocked discard reason", "mocked discard reason description");

        this.apiClientMock
          .Setup(client => client.DiscardMaterialAsync(request, cmsAuthValues))
          .ThrowsAsync(exceptionMocked);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await this.communicationService.DiscardMaterialAsync(
                caseId,
                request.materialId,
                request.discardReason,
                request.discardReasonDescription,
                cmsAuthValues,
                request.id)
            .ConfigureAwait(false));

        Assert.Contains(this.mockLogger.Logs, log =>
          log.LogLevel == LogLevel.Information &&
          log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Attempting to discard material with materialId [1212]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] Failed to discard material with materialId [1212]"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.GetUsedMgFormsAsync(int, CmsAuthValues)"/>
    /// returns used MG forms when the API returns valid used MG forms.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetUsedMgFormsAsync_ShouldReturn_WhenApiReturnsUsedMgForms()
    {
        // Arrange
        int caseId = 1234;
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);

        var mgForm1 = new MgForm(
                Id: 1,
                Title: "MG Form One",
                OriginalFileName: "mgForm1.pdf",
                MaterialType: "1202",
                DocumentType: 2,
                Link: "http://example1.com",
                Status: "Used",
                Date: this.receivedDate);

        var mgForm2 = new MgForm(
                Id: 2,
                Title: "MG Form Two",
                OriginalFileName: "mgForm2.pdf",
                MaterialType: "1203",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "Used",
                Date: this.receivedDate);

        var mgForms = new List<MgForm>
        {
            mgForm1,
            mgForm2,
        };

        var apiUsedMgForms = new UsedMgFormsResponse
        {
            MgForms = mgForms,
        };

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        this.apiClientMock
            .Setup(client => client.GetUsedMgFormsAsync(It.IsAny<GetUsedMgFormsRequest>(), cmsAuthValues))
            .ReturnsAsync(apiUsedMgForms);

        // Act
        UsedMgFormsResponse result = await this.communicationService.GetUsedMgFormsAsync(caseId, cmsAuthValues);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.MgForms);
        Assert.Equal(2, result.MgForms.Count);
        Assert.Equal("mgForm1.pdf", result.MgForms[0].OriginalFileName);
        Assert.Equal("mgForm2.pdf", result.MgForms[1].OriginalFileName);
        Assert.Equal("Used", result.MgForms[0].Status);
        Assert.Equal("Used", result.MgForms[1].Status);
        Assert.Equal(this.receivedDate, result.MgForms[0].Date);
        Assert.Equal(this.receivedDate, result.MgForms[1].Date);

        this.apiClientMock.Verify(client => client.GetUsedMgFormsAsync(It.Is<GetUsedMgFormsRequest>(r => r.CaseId == caseId), cmsAuthValues), Times.Once);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Fetching used MG forms for caseId [{caseIdString}]"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.GetUsedMgFormsAsync(int, CmsAuthValues)"/>
    /// logs an error and rethrows the exception when the API throws an exception.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetUsedMgFormsAsync_ShouldLogErrorAndRethrow_WhenApiThrowsException()
    {
        // Arrange
        int caseId = 1234;
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);
        var exception = new Exception("API Error");

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        this.apiClientMock
            .Setup(client => client.GetUsedMgFormsAsync(It.IsAny<GetUsedMgFormsRequest>(), cmsAuthValues))
            .ThrowsAsync(exception);

        // Act & Assert
        Exception ex = await Assert.ThrowsAsync<Exception>(() => this.communicationService.GetUsedMgFormsAsync(caseId, cmsAuthValues));
        Assert.Equal("API Error", ex.Message);

        this.apiClientMock.Verify(client => client.GetUsedMgFormsAsync(It.Is<GetUsedMgFormsRequest>(r => r.CaseId == caseId), cmsAuthValues), Times.Once);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Fetching used MG forms for caseId [{caseIdString}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching used MG forms for caseId [{caseIdString}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains("API Error"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.GetUsedOtherMaterialsAsync(int, CmsAuthValues)"/>
    /// returns used other materials when the API returns valid used other materials.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetUsedOtherMaterialsAsync_ShouldReturn_WhenApiReturnsUsedOtherMaterials()
    {
        // Arrange
        int caseId = 1234;
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);

        var otherMaterial1 = new MgForm(
                Id: 1,
                Title: "Other Material One",
                OriginalFileName: "otherMaterial1.pdf",
                MaterialType: "1204",
                DocumentType: 2,
                Link: "http://example1.com",
                Status: "Used",
                Date: this.receivedDate);

        var otherMaterial2 = new MgForm(
                Id: 2,
                Title: "Other Material Two",
                OriginalFileName: "otherMaterial2.pdf",
                MaterialType: "1205",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "Used",
                Date: this.receivedDate);

        var otherMaterials = new List<MgForm>
        {
            otherMaterial1,
            otherMaterial2,
        };

        var apiUsedOtherMaterials = new UsedOtherMaterialsResponse
        {
            MgForms = otherMaterials,
        };

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        this.apiClientMock
            .Setup(client => client.GetUsedOtherMaterialsAsync(It.IsAny<GetUsedOtherMaterialsRequest>(), cmsAuthValues))
            .ReturnsAsync(apiUsedOtherMaterials);

        // Act
        UsedOtherMaterialsResponse result = await this.communicationService.GetUsedOtherMaterialsAsync(caseId, cmsAuthValues);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.MgForms);
        Assert.Equal(2, result.MgForms.Count);
        Assert.Equal("otherMaterial1.pdf", result.MgForms[0].OriginalFileName);
        Assert.Equal("otherMaterial2.pdf", result.MgForms[1].OriginalFileName);
        Assert.Equal("Used", result.MgForms[0].Status);
        Assert.Equal("Used", result.MgForms[1].Status);
        Assert.Equal(this.receivedDate, result.MgForms[0].Date);
        Assert.Equal(this.receivedDate, result.MgForms[1].Date);

        this.apiClientMock.Verify(client => client.GetUsedOtherMaterialsAsync(It.Is<GetUsedOtherMaterialsRequest>(r => r.CaseId == caseId), cmsAuthValues), Times.Once);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Fetching used other materials for caseId [{caseIdString}]"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.GetUsedOtherMaterialsAsync(int, CmsAuthValues)"/>
    /// logs an error and rethrows the exception when the API throws an exception.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetUsedOtherMaterialsAsync_ShouldLogErrorAndRethrow_WhenApiThrowsException()
    {
        // Arrange
        int caseId = 1234;
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);
        var exception = new Exception("API Error");

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        this.apiClientMock
            .Setup(client => client.GetUsedOtherMaterialsAsync(It.IsAny<GetUsedOtherMaterialsRequest>(), cmsAuthValues))
            .ThrowsAsync(exception);

        // Act & Assert
        Exception ex = await Assert.ThrowsAsync<Exception>(() => this.communicationService.GetUsedOtherMaterialsAsync(caseId, cmsAuthValues));
        Assert.Equal("API Error", ex.Message);

        this.apiClientMock.Verify(client => client.GetUsedOtherMaterialsAsync(It.Is<GetUsedOtherMaterialsRequest>(r => r.CaseId == caseId), cmsAuthValues), Times.Once);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Fetching used other materials for caseId [{caseIdString}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching used other materials for caseId [{caseIdString}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains("API Error"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.GetUsedMgFormsAsync(int, CmsAuthValues)"/>
    /// returns an empty response when the API returns no MG forms.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetUsedMgFormsAsync_ShouldReturnEmptyResponse_WhenApiReturnsNoMgForms()
    {
        // Arrange
        int caseId = 1234;
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);

        var apiUsedMgForms = new UsedMgFormsResponse
        {
            MgForms = new List<MgForm>(), // Empty list
        };

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        this.apiClientMock
            .Setup(client => client.GetUsedMgFormsAsync(It.IsAny<GetUsedMgFormsRequest>(), cmsAuthValues))
            .ReturnsAsync(apiUsedMgForms);

        // Act
        UsedMgFormsResponse result = await this.communicationService.GetUsedMgFormsAsync(caseId, cmsAuthValues);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.MgForms);
        Assert.Empty(result.MgForms);

        this.apiClientMock.Verify(client => client.GetUsedMgFormsAsync(It.Is<GetUsedMgFormsRequest>(r => r.CaseId == caseId), cmsAuthValues), Times.Once);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Fetching used MG forms for caseId [{caseIdString}]"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.GetUsedOtherMaterialsAsync(int, CmsAuthValues)"/>
    /// returns an empty response when the API returns no other materials.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task GetUsedOtherMaterialsAsync_ShouldReturnEmptyResponse_WhenApiReturnsNoOtherMaterials()
    {
        // Arrange
        int caseId = 1234;
        string caseIdString = caseId.ToString(CultureInfo.InvariantCulture);

        var apiUsedOtherMaterials = new UsedOtherMaterialsResponse
        {
            MgForms = new List<MgForm>(), // Empty list
        };

        var cmsAuthValues = new CmsAuthValues(
            "cookies",
            "token",
            Guid.NewGuid());

        this.apiClientMock
            .Setup(client => client.GetUsedOtherMaterialsAsync(It.IsAny<GetUsedOtherMaterialsRequest>(), cmsAuthValues))
            .ReturnsAsync(apiUsedOtherMaterials);

        // Act
        UsedOtherMaterialsResponse result = await this.communicationService.GetUsedOtherMaterialsAsync(caseId, cmsAuthValues);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.MgForms);
        Assert.Empty(result.MgForms);

        this.apiClientMock.Verify(client => client.GetUsedOtherMaterialsAsync(It.Is<GetUsedOtherMaterialsRequest>(r => r.CaseId == caseId), cmsAuthValues), Times.Once);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Fetching used other materials for caseId [{caseIdString}]"));
    }

    /// <summary>
    /// Tests the result against expexted when API calls succeeds.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task GetExhibitProducersAsync_ReturnsExhibitProducersForCase_WhenApiCallIsSuccessful()
    {
        // Arrange
        int caseId = 4321;
        var cmsAuthValues = new CmsAuthValues("validCmsToken", "validCmsCookies");

        var expectedProducers = new ExhibitProducersResponse
        {
            ExhibitProducers = new List<ExhibitProducer>()
            {
                new (Id: 343, "Joe SMITH", false),
                new (Id: 343, "Bob JACKSON", false),
            },
        };

        this.apiClientMock
            .Setup(x => x.GetExhibitProducersAsync(It.IsAny<GetExhibitProducersRequest>(), cmsAuthValues))
            .ReturnsAsync(expectedProducers);

        // Act
        var result = await this.communicationService.GetExhibitProducersAsync(caseId, cmsAuthValues);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<ExhibitProducersResponse>(result);
        Assert.Equal(2, result.ExhibitProducers?.Count);
        Assert.Equal(expectedProducers.ExhibitProducers.First().Id, result.ExhibitProducers?.First().Id);
        Assert.Equal(expectedProducers.ExhibitProducers.First().Name, result.ExhibitProducers?.First().Name);

        Assert.Contains(this.mockLogger.Logs, log =>
           log.LogLevel == LogLevel.Information &&
           log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Getting exhibit producers for caseId [{caseId}]"));
    }

    /// <summary>
    /// Tests the result against expexted when API calls succeeds.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task GetExhibitProducersAsync_ThrowsException_WhenApiCallFails()
    {
        // Arrange
        int caseId = 4321;
        var cmsAuthValues = new CmsAuthValues("validCmsToken", "validCmsCookies");

        var expectedProducers = new ExhibitProducersResponse
        {
            ExhibitProducers = new List<ExhibitProducer>()
            {
                new (Id: 343, "Joe SMITH", false),
                new (Id: 343, "Bob JACKSON", false),
            },
        };

        this.apiClientMock
            .Setup(x => x.GetExhibitProducersAsync(It.IsAny<GetExhibitProducersRequest>(), cmsAuthValues))
            .ThrowsAsync(new Exception("DDEI-EAS API error."));

        // Act
        Exception exception = await Assert.ThrowsAsync<Exception>(() => this.communicationService.GetExhibitProducersAsync(caseId, cmsAuthValues));

        Assert.Contains(this.mockLogger.Logs, log =>
        log.LogLevel == LogLevel.Information &&
        log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Getting exhibit producers for caseId [{caseId}]"));

        Assert.Equal("DDEI-EAS API error.", exception.Message);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching producers for caseId [{caseId}]"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.UpdateStatementAsync(int, EditStatementRequest, CmsAuthValues, Guid)"/>
    /// returns expected response after updating a statement.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateStatementAsync_ShouldLogAndReturn_WhenApiReturnsSuccess()
    {
        // Arrange
        var cmsAuthValues = new CmsAuthValues(
           "cookies",
           "token",
           Guid.NewGuid());

        var expectedResponse = new UpdateStatementResponse(new UpdateStatementData
        {
            Id = 1212,
        });

        int caseId = 134;
        var request = new UpdateStatementRequest(Guid.NewGuid(), 4545, 1212, 2322, new DateOnly(2025, 09, 18), 4, true);

        this.apiClientMock
          .Setup(client => client.UpdateStatementAsync(It.IsAny<UpdateStatementRequest>(), It.IsAny<CmsAuthValues>()))
          .ReturnsAsync(expectedResponse);

        // Act
        UpdateStatementResponse result = await this.communicationService.UpdateStatementAsync(
            caseId,
            request,
            cmsAuthValues,
            request.id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);

        Assert.Contains(this.mockLogger.Logs, log =>
          log.LogLevel == LogLevel.Information &&
          log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Attempting to update statement with materialId [{expectedResponse?.UpdateStatementData?.Id}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] successfully updated statement with materialId [{expectedResponse?.UpdateStatementData?.Id}]"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.UpdateStatementAsync(int, EditStatementRequest, CmsAuthValues, Guid)"/>
    /// logs and rethrows the exception when the API call fails.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateStatementAsync_ShouldLogAndRethrowException_WhenApiCallFails()
    {
        // Arrange
        var cmsAuthValues = new CmsAuthValues(
           "cookies",
           "token",
           Guid.NewGuid());

        int caseId = 134;

        var exceptionMocked = new BadHttpRequestException("Material does not exists in DDEI");

        var request = new UpdateStatementRequest(Guid.NewGuid(), 4545, 4533, 2322, new DateOnly(2025, 09, 18), 4, true);

        this.apiClientMock
          .Setup(client => client.UpdateStatementAsync(It.IsAny<UpdateStatementRequest>(), It.IsAny<CmsAuthValues>()))
          .ThrowsAsync(exceptionMocked);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await this.communicationService.UpdateStatementAsync(
                caseId,
                request,
                cmsAuthValues,
                request.id)
            .ConfigureAwait(false));

        Assert.Contains(this.mockLogger.Logs, log =>
          log.LogLevel == LogLevel.Information &&
          log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Attempting to update statement with materialId [4533]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] Failed to update statement with materialId [4533]"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.UpdateExhibitAsync(int, EditExhibitRequest, CmsAuthValues, Guid)"/>
    /// returns expected response after updating an exhibit.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateExhibitAsync_ShouldLogAndReturn_WhenApiReturnsSuccess()
    {
        // Arrange
        var cmsAuthValues = new CmsAuthValues(
           "cookies",
           "token",
           Guid.NewGuid());

        var expectedResponse = new UpdateExhibitResponse(new UpdateExhibitData
        {
            Id = 4343,
        });

        int caseId = 134;
        var request = new UpdateExhibitRequest(Guid.NewGuid(), 4545, 3434, "some-item", 4343, "some-ref", "some-sub", true, "new-producer", null);

        this.apiClientMock
          .Setup(client => client.UpdateExhibitAsync(It.IsAny<UpdateExhibitRequest>(), It.IsAny<CmsAuthValues>()))
          .ReturnsAsync(expectedResponse);

        // Act
        UpdateExhibitResponse result = await this.communicationService.UpdateExhibitAsync(
            caseId,
            new UpdateExhibitRequest(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), 4343, "", "", true, "", null),
            cmsAuthValues,
            request.id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);

        Assert.Contains(this.mockLogger.Logs, log =>
          log.LogLevel == LogLevel.Information &&
          log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Attempting to update exhibit with materialId [{expectedResponse?.UpdateExhibitData?.Id}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] successfully updated exhibit with materialId [{expectedResponse?.UpdateExhibitData?.Id}]"));
    }

    /// <summary>
    /// Tests that <see cref="CommunicationService.UpdateExhibitAsync(int, EditExhibitRequest, CmsAuthValues, Guid)"/>
    /// logs and rethrows the exception when the API call fails.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Fact]
    public async Task UpdateExhibitAsync_ShouldLogAndRethrowException_WhenApiCallFails()
    {
        // Arrange
        var cmsAuthValues = new CmsAuthValues(
           "cookies",
           "token",
           Guid.NewGuid());

        int caseId = 134;

        var exceptionMocked = new BadHttpRequestException("Material does not exists in DDEI");

        var request = new UpdateExhibitRequest(Guid.NewGuid(), 4545, 3434, "some-item", 4343, "some-ref", "some-sub", true, "new-producer", null);

        this.apiClientMock
          .Setup(client => client.UpdateExhibitAsync(It.IsAny<UpdateExhibitRequest>(), It.IsAny<CmsAuthValues>()))
          .ThrowsAsync(exceptionMocked);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(async () =>
            await this.communicationService.UpdateExhibitAsync(
                caseId,
                request,
                cmsAuthValues,
                request.id)
            .ConfigureAwait(false));

        Assert.Contains(this.mockLogger.Logs, log =>
          log.LogLevel == LogLevel.Information &&
          log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Attempting to update exhibit with materialId [4343]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] Failed to update exhibit with materialId [4343]"));
    }
}
