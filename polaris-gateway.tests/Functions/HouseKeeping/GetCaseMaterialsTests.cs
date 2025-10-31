// <copyright file="GetCaseMaterialsTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions.HouseKeeping;

using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Cps.Fct.Hk.Ui.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using PolarisGateway.Functions.HouseKeeping;
using Common.Dto.Response.HouseKeeping;
using Common.Dto.Request;
using Common.Constants;

/// <summary>
/// Unit tests for the <see cref="GetCaseMaterials"/> class.
/// </summary>
public class GetCaseMaterialsTests
{
    private readonly TestLogger<GetCaseMaterials> mockLogger;
    private readonly Mock<ICommunicationService> mockCommunicationService;
    private readonly Mock<ICaseMaterialService> mockCaseMaterialService;
    private readonly GetCaseMaterials getCaseMaterialsFunction;
    private readonly DateTime receivedDate = new DateTime(2025, 04, 01);
    private readonly DateTime statementTakenDate = new DateTime(2025, 03, 02);

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCaseMaterialsTests"/> class.
    /// </summary>
    public GetCaseMaterialsTests()
    {
        // Initialize mocks
        mockLogger = new TestLogger<GetCaseMaterials>();
        mockCommunicationService = new Mock<ICommunicationService>();
        mockCaseMaterialService = new Mock<ICaseMaterialService>();

        // Initialize the function class
        getCaseMaterialsFunction = new GetCaseMaterials(
            mockLogger,
            mockCommunicationService.Object,
            mockCaseMaterialService.Object);
    }


    /// <summary>
    /// Tests that the function returns an OK result when a valid request is provided.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsOkResult_WhenValidRequestProvided()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        var mockCaseMaterials = new List<CaseMaterial>
        {
            new CaseMaterial(1, "FileA.pdf", "Subject A", 1012, 123, "/some/path/doc1.pdf", "Administrative", "Type A", false, "None"),
            new CaseMaterial(2, "FileB.pdf", "Subject B", 1034, 456, "/some/path/doc2.pdf", "Evidential", "Type B", false, "None"),
        };

        var mockCommunications = new List<Communication>
        {
            new Communication(1, "FileA.pdf", "Subject A", 1012, 123, "/some/path/doc1.pdf", "None", "Administrative", "Type A", false),
            new Communication(2, "FileB.pdf", "Subject B", 1034, 456, "/some/path/doc2.pdf", "None", "Evidential", "Type B", false),
        };

        // Mock empty responses for unused materials, used statements, used exhibits and used MG forms
        var unusedMaterials = new UnusedMaterialsResponse();
        var usedStatements = new Common.Dto.Response.HouseKeeping.UsedStatementsResponse();
        var usedExhibits = new UsedExhibitsResponse();
        var usedMgForms = new UsedMgFormsResponse();
        var usedOtherMaterials = new UsedOtherMaterialsResponse();
        var exhibitProducers = new ExhibitProducersResponse();

        // Mock RetrieveCaseMaterialsAsync to return all case materials
        mockCaseMaterialService
            .Setup(x => x.RetrieveCaseMaterialsAsync(123, It.IsAny<CmsAuthValues>()))
            .ReturnsAsync((mockCommunications, unusedMaterials, usedStatements, usedExhibits, usedMgForms, usedOtherMaterials, exhibitProducers));

        // Mock MapCommunicationsToCaseMaterials to return all case materials
        mockCaseMaterialService
            .Setup(x => x.MapCommunicationsToCaseMaterials(mockCommunications))
            .Returns(mockCaseMaterials);

        // Act
        IActionResult result = await getCaseMaterialsFunction.Run(mockRequest.Object, 123);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result); // Verify we got an OkObjectResult

        Assert.NotNull(okResult.Value);

        List<CaseMaterial> caseMaterials = Assert.IsType<List<CaseMaterial>>(okResult.Value);

        Assert.Equal(mockCaseMaterials, caseMaterials);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseMaterials function processed a request"));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [123] GetCaseMaterials function completed"));
    }


    /// <summary>
    /// Tests that the function returns an unprocessable error when GetCommunications is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityError_WhenGetCommunications_IsNull()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        DateTime receivedDate = new DateTime(2025, 04, 01);
        DateTime statementTakenDate = new DateTime(2025, 03, 02);

        // Communications
        IReadOnlyCollection<Communication> mockCommunications = null;

        // Exhibits
        var exhibit1 = new Exhibit(
                Id: 1,
                Title: "Exhibit One",
                OriginalFileName: "exhibit1.pdf",
                MaterialType: "1202",
                DocumentType: 2,
                Link: "http://example1.com",
                Status: "None",
                ReceivedDate: receivedDate,
                Reference: "some-reference",
                Producer: "some-producer");

        var exhibit2 = new Exhibit(
                Id: 2,
                Title: "Exhibit Two",
                OriginalFileName: "exhibit2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "None",
                ReceivedDate: receivedDate,
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
                Status: "None",
                Date: receivedDate);

        var mgForm2 = new MgForm(
                Id: 4,
                Title: "MgForm Two",
                OriginalFileName: "mgForm2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "None",
                Date: receivedDate);

        var mgForms = new List<MgForm>
        {
            mgForm1,
            mgForm2,
        };

        // OtherMaterials
        var otherMaterial1 = new MgForm(
                Id: 5,
                Title: "OtherMaterial One",
                OriginalFileName: "otherMaterial1.pdf",
                MaterialType: "1202",
                DocumentType: 2,
                Link: "http://example1.com",
                Status: "None",
                Date: receivedDate);

        var otherMaterial2 = new MgForm(
                Id: 6,
                Title: "OtherMaterial Two",
                OriginalFileName: "otherMaterial2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "None",
                Date: receivedDate);

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
                Status: "None",
                ReceivedDate: receivedDate,
                StatementTakenDate: statementTakenDate);

        var statement2 = new Statement(
                Id: 8,
                WitnessId: 789,
                Title: "Statement Two",
                OriginalFileName: "statement2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "None",
                ReceivedDate: receivedDate,
                StatementTakenDate: statementTakenDate);

        var statements = new List<Statement>
        {
            statement1,
            statement2,
        };

        // UnusedMaterials
        var mockUnusedMaterials = new UnusedMaterialsResponse
        {
            Exhibits = exhibits,
            MgForms = mgForms,
            OtherMaterials = otherMaterials,
            Statements = statements,
        };

        // UsedStatements
        var mockUsedStatements = new Common.Dto.Response.HouseKeeping.UsedStatementsResponse
        {
            Statements = statements,
        };

        // UsedExhibits
        var mockUsedExhibits = new UsedExhibitsResponse
        {
            Exhibits = exhibits,
        };


        mockCommunicationService
            .Setup(x => x.GetCommunicationsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .Returns(Task.FromResult(mockCommunications));

        mockCommunicationService
            .Setup(x => x.GetUnusedMaterialsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .Returns(Task.FromResult(mockUnusedMaterials));

        mockCommunicationService
            .Setup(x => x.GetUsedStatementsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .Returns(Task.FromResult(mockUsedStatements));

        mockCommunicationService
            .Setup(x => x.GetUsedExhibitsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .Returns(Task.FromResult(mockUsedExhibits));

        // Act
        IActionResult result = await getCaseMaterialsFunction.Run(mockRequest.Object, 123);

        // Assert
        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseMaterials function processed a request"));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseMaterials function encountered an unprocessable entity error: " +
            "Failed to retrieve case materials for caseId [123]"));
    }

    /// <summary>
    /// Tests that the function returns an unprocessable error when GetUnusedMaterials is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityError_WhenUnusedMaterials_IsNull()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        // Communications
        var mockCommunications = new List<Communication>
        {
            new Communication(1, "FileA.pdf", "Subject A", 1012, 123, "/some/path/doc1.pdf", "None", "Administrative", "Type A", false),
            new Communication(2, "FileB.pdf", "Subject B", 1034, 456, "/some/path/doc2.pdf", "None", "Evidential", "Type B", false),
        };

        // Exhibits
        var exhibit1 = new Exhibit(
                Id: 1,
                Title: "Exhibit One",
                OriginalFileName: "exhibit1.pdf",
                MaterialType: "1202",
                DocumentType: 2,
                Link: "http://example1.com",
                Status: "None",
                ReceivedDate: receivedDate,
                Reference: "some-reference",
                Producer: "some-producer");

        var exhibit2 = new Exhibit(
                Id: 2,
                Title: "Exhibit Two",
                OriginalFileName: "exhibit2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "None",
                ReceivedDate: receivedDate,
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
                Status: "None",
                Date: receivedDate);

        var mgForm2 = new MgForm(
                Id: 4,
                Title: "MgForm Two",
                OriginalFileName: "mgForm2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "None",
                Date: receivedDate);

        var mgForms = new List<MgForm>
        {
            mgForm1,
            mgForm2,
        };

        // OtherMaterials
        var otherMaterial1 = new MgForm(
                Id: 5,
                Title: "OtherMaterial One",
                OriginalFileName: "otherMaterial1.pdf",
                MaterialType: "1202",
                DocumentType: 2,
                Link: "http://example1.com",
                Status: "None",
                Date: receivedDate);

        var otherMaterial2 = new MgForm(
                Id: 6,
                Title: "OtherMaterial Two",
                OriginalFileName: "otherMaterial2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "None",
                Date: receivedDate);

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
                Status: "None",
                ReceivedDate: receivedDate,
                StatementTakenDate: statementTakenDate);

        var statement2 = new Statement(
                Id: 8,
                WitnessId: 789,
                Title: "Statement Two",
                OriginalFileName: "statement2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "None",
                ReceivedDate: receivedDate,
                StatementTakenDate: statementTakenDate);

        var statements = new List<Statement>
        {
            statement1,
            statement2,
        };

        // UnusedMaterials
        UnusedMaterialsResponse mockUnusedMaterials = null;

        // UsedStatements
        var mockUsedStatements = new Common.Dto.Response.HouseKeeping.UsedStatementsResponse
        {
            Statements = statements,
        };

        // UsedExhibits
        var mockUsedExhibits = new UsedExhibitsResponse
        {
            Exhibits = exhibits,
        };

        mockCommunicationService
            .Setup(x => x.GetCommunicationsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .Returns(Task.FromResult((IReadOnlyCollection<Communication>)mockCommunications));

        mockCommunicationService
            .Setup(x => x.GetUnusedMaterialsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .Returns(Task.FromResult(mockUnusedMaterials));

        mockCommunicationService
            .Setup(x => x.GetUsedStatementsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .Returns(Task.FromResult(mockUsedStatements));

        mockCommunicationService
            .Setup(x => x.GetUsedExhibitsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .Returns(Task.FromResult(mockUsedExhibits));

        // Act
        IActionResult result = await getCaseMaterialsFunction.Run(mockRequest.Object, 123);

        // Assert
        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseMaterials function processed a request"));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseMaterials function encountered an unprocessable entity error: " +
            "Failed to retrieve case materials for caseId [123]"));
    }

    /// <summary>
    /// Tests that the function returns an unprocessable error when GetUsedStatements is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityError_WhenUsedStatements_IsNull()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        // Communications
        var mockCommunications = new List<Communication>
        {
            new Communication(1, "FileA.pdf", "Subject A", 1012, 123, "/some/path/doc1.pdf", "None", "Administrative", "Type A", false),
            new Communication(2, "FileB.pdf", "Subject B", 1034, 456, "/some/path/doc2.pdf", "None", "Evidential", "Type B", false),
        };

        // Exhibits
        var exhibit1 = new Exhibit(
                Id: 1,
                Title: "Exhibit One",
                OriginalFileName: "exhibit1.pdf",
                MaterialType: "1202",
                DocumentType: 2,
                Link: "http://example1.com",
                Status: "None",
                ReceivedDate: receivedDate,
                Reference: "some-reference",
                Producer: "some-producer");

        var exhibit2 = new Exhibit(
                Id: 2,
                Title: "Exhibit Two",
                OriginalFileName: "exhibit2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "None",
                ReceivedDate: receivedDate,
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
                Status: "None",
                Date: receivedDate);

        var mgForm2 = new MgForm(
                Id: 4,
                Title: "MgForm Two",
                OriginalFileName: "mgForm2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "None",
                Date: receivedDate);

        var mgForms = new List<MgForm>
        {
            mgForm1,
            mgForm2,
        };

        // OtherMaterials
        var otherMaterial1 = new MgForm(
                Id: 5,
                Title: "OtherMaterial One",
                OriginalFileName: "otherMaterial1.pdf",
                MaterialType: "1202",
                DocumentType: 2,
                Link: "http://example1.com",
                Status: "None",
                Date: receivedDate);

        var otherMaterial2 = new MgForm(
                Id: 6,
                Title: "OtherMaterial Two",
                OriginalFileName: "otherMaterial2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "None",
                Date: receivedDate);

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
                Status: "None",
                ReceivedDate: receivedDate,
                StatementTakenDate: statementTakenDate);

        var statement2 = new Statement(
                Id: 8,
                WitnessId: 789,
                Title: "Statement Two",
                OriginalFileName: "statement2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "None",
                ReceivedDate: receivedDate,
                StatementTakenDate: statementTakenDate);

        var statements = new List<Statement>
        {
            statement1,
            statement2,
        };

        // UnusedMaterials
        var mockUnusedMaterials = new UnusedMaterialsResponse
        {
            Exhibits = exhibits,
            MgForms = mgForms,
            OtherMaterials = otherMaterials,
            Statements = statements,
        };

        // UsedStatements
        Common.Dto.Response.HouseKeeping.UsedStatementsResponse mockUsedStatements = null;

        // UsedExhibits
        var mockUsedExhibits = new UsedExhibitsResponse
        {
            Exhibits = exhibits,
        };

        mockCommunicationService
            .Setup(x => x.GetCommunicationsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .Returns(Task.FromResult((IReadOnlyCollection<Communication>)mockCommunications));

        mockCommunicationService
            .Setup(x => x.GetUnusedMaterialsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .Returns(Task.FromResult(mockUnusedMaterials));

        mockCommunicationService
            .Setup(x => x.GetUsedStatementsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .Returns(Task.FromResult(mockUsedStatements));

        mockCommunicationService
            .Setup(x => x.GetUsedExhibitsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .Returns(Task.FromResult(mockUsedExhibits));

        // Act
        IActionResult result = await getCaseMaterialsFunction.Run(mockRequest.Object, 123);

        // Assert
        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseMaterials function processed a request"));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseMaterials function encountered an unprocessable entity error: " +
            "Failed to retrieve case materials for caseId [123]"));
    }

    /// <summary>
    /// Tests that the function returns an unprocessable error when GetUsedExhibits is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityError_WhenUsedExhibits_IsNull()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        // Communications
        var mockCommunications = new List<Communication>
        {
            new Communication(1, "FileA.pdf", "Subject A", 1012, 123, "/some/path/doc1.pdf", "None", "Administrative", "Type A", false),
            new Communication(2, "FileB.pdf", "Subject B", 1034, 456, "/some/path/doc2.pdf", "None", "Evidential", "Type B", false),
        };

        // Exhibits
        var exhibit1 = new Exhibit(
                Id: 1,
                Title: "Exhibit One",
                OriginalFileName: "exhibit1.pdf",
                MaterialType: "1202",
                DocumentType: 2,
                Link: "http://example1.com",
                Status: "None",
                ReceivedDate: receivedDate,
                Reference: "some-reference",
                Producer: "some-producer");

        var exhibit2 = new Exhibit(
                Id: 2,
                Title: "Exhibit Two",
                OriginalFileName: "exhibit2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "None",
                ReceivedDate: receivedDate,
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
                Status: "None",
                Date: receivedDate);

        var mgForm2 = new MgForm(
                Id: 4,
                Title: "MgForm Two",
                OriginalFileName: "mgForm2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "None",
                Date: receivedDate);

        var mgForms = new List<MgForm>
        {
            mgForm1,
            mgForm2,
        };

        // OtherMaterials
        var otherMaterial1 = new MgForm(
                Id: 5,
                Title: "OtherMaterial One",
                OriginalFileName: "otherMaterial1.pdf",
                MaterialType: "1202",
                DocumentType: 2,
                Link: "http://example1.com",
                Status: "None",
                Date: receivedDate);

        var otherMaterial2 = new MgForm(
                Id: 6,
                Title: "OtherMaterial Two",
                OriginalFileName: "otherMaterial2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "None",
                Date: receivedDate);

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
                Status: "None",
                ReceivedDate: receivedDate,
                StatementTakenDate: statementTakenDate);

        var statement2 = new Statement(
                Id: 8,
                WitnessId: 789,
                Title: "Statement Two",
                OriginalFileName: "statement2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "None",
                ReceivedDate: receivedDate,
                StatementTakenDate: statementTakenDate);

        var statements = new List<Statement>
        {
            statement1,
            statement2,
        };

        // UnusedMaterials
        var mockUnusedMaterials = new UnusedMaterialsResponse
        {
            Exhibits = exhibits,
            MgForms = mgForms,
            OtherMaterials = otherMaterials,
            Statements = statements,
        };

        // UsedStatements
        var mockUsedStatements = new Common.Dto.Response.HouseKeeping.UsedStatementsResponse
        {
            Statements = statements,
        };

        // UsedExhibits
        UsedExhibitsResponse mockUsedExhibits = null;

        mockCommunicationService
            .Setup(x => x.GetCommunicationsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .Returns(Task.FromResult((IReadOnlyCollection<Communication>)mockCommunications));

        mockCommunicationService
            .Setup(x => x.GetUnusedMaterialsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .Returns(Task.FromResult(mockUnusedMaterials));

        mockCommunicationService
            .Setup(x => x.GetUsedStatementsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .Returns(Task.FromResult(mockUsedStatements));

        mockCommunicationService
            .Setup(x => x.GetUsedExhibitsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .Returns(Task.FromResult(mockUsedExhibits));

        // Act
        IActionResult result = await getCaseMaterialsFunction.Run(mockRequest.Object, 123);

        // Assert
        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseMaterials function processed a request"));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseMaterials function encountered an unprocessable entity error: " +
            "Failed to retrieve case materials for caseId [123]"));
    }
#pragma warning restore CS8604 // Possible null reference argument.

    /// <summary>
    /// Tests that the function returns an unprocessable error when an exception is thrown by GetAttachments.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityError_WhenExceptionIsThrownBy_GetAllAttachments()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        var mockCommunications = new List<Communication>
        {
            new Communication(1, "FileA.pdf", "Subject A", 1012, 123, "/some/path/doc1.pdf", "None", "Administrative", "Type A", false),
            new Communication(2, "FileB.pdf", "Subject B", 1034, 456, "/some/path/doc2.pdf", "None", "Evidential", "Type B", false),
        };

        // Mock attachments including the required DocumentTypeId
        var mockAttachments = new List<Attachment>
        {
            new Attachment(101, "Attachment A", "Description A", "/path/to/attachmentA", "Class A", 5, null, null, null, "Tag A", 123, "AttachmentA.pdf", "UserA", 111, "Processed", "DirectionA"),
            new Attachment(102, "Attachment B", "Description B", "/path/to/attachmentB", "Class B", 10, null, null, null, "Tag B", 456, "AttachmentB.pdf", "UserB", 222, "NotProcessed", "DirectionB"),
        };

        // Exhibits
        var exhibit1 = new Exhibit(
                Id: 1,
                Title: "Exhibit One",
                OriginalFileName: "exhibit1.pdf",
                MaterialType: "1202",
                DocumentType: 2,
                Link: "http://example1.com",
                Status: "None",
                ReceivedDate: receivedDate,
                Reference: "some-reference",
                Producer: "some-producer");

        var exhibit2 = new Exhibit(
                Id: 2,
                Title: "Exhibit Two",
                OriginalFileName: "exhibit2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "None",
                ReceivedDate: receivedDate,
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
                Status: "None",
                Date: receivedDate);

        var mgForm2 = new MgForm(
                Id: 4,
                Title: "MgForm Two",
                OriginalFileName: "mgForm2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "None",
                Date: receivedDate);

        var mgForms = new List<MgForm>
        {
            mgForm1,
            mgForm2,
        };

        // OtherMaterials
        var otherMaterial1 = new MgForm(
                Id: 5,
                Title: "OtherMaterial One",
                OriginalFileName: "otherMaterial1.pdf",
                MaterialType: "1202",
                DocumentType: 2,
                Link: "http://example1.com",
                Status: "None",
                Date: receivedDate);

        var otherMaterial2 = new MgForm(
                Id: 6,
                Title: "OtherMaterial Two",
                OriginalFileName: "otherMaterial2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "None",
                Date: receivedDate);

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
                Status: "None",
                ReceivedDate: receivedDate,
                StatementTakenDate: statementTakenDate);

        var statement2 = new Statement(
                Id: 8,
                WitnessId: 789,
                Title: "Statement Two",
                OriginalFileName: "statement2.pdf",
                MaterialType: "1202",
                DocumentType: 3,
                Link: "http://example2.com",
                Status: "None",
                ReceivedDate: receivedDate,
                StatementTakenDate: statementTakenDate);

        var statements = new List<Statement>
        {
            statement1,
            statement2,
        };

        // UnusedMaterials
        var mockUnusedMaterials = new UnusedMaterialsResponse
        {
            Exhibits = exhibits,
            MgForms = mgForms,
            OtherMaterials = otherMaterials,
            Statements = statements,
        };

        // UsedStatements
        var mockUsedStatements = new Common.Dto.Response.HouseKeeping.UsedStatementsResponse
        {
            Statements = statements,
        };

        // UsedExhibits
        var mockUsedExhibits = new UsedExhibitsResponse
        {
            Exhibits = exhibits,
        };

        // Mock empty responses for unused materials, used statements, used exhibits and used MG forms
        var unusedMaterials = new UnusedMaterialsResponse();
        var usedStatements = new Common.Dto.Response.HouseKeeping.UsedStatementsResponse();
        var usedExhibits = new UsedExhibitsResponse();
        var usedMgForms = new UsedMgFormsResponse();
        var usedOtherMaterials = new UsedOtherMaterialsResponse();
        var exhibitProducers = new ExhibitProducersResponse();

        mockCaseMaterialService
            .Setup(x => x.RetrieveCaseMaterialsAsync(123, It.IsAny<CmsAuthValues>()))
            .ReturnsAsync((mockCommunications, unusedMaterials, usedStatements, usedExhibits, usedMgForms, usedOtherMaterials, exhibitProducers));

        mockCommunicationService
            .Setup(x => x.GetCommunicationsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .Returns(Task.FromResult((IReadOnlyCollection<Communication>)mockCommunications));

        mockCommunicationService
            .Setup(x => x.GetUnusedMaterialsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .Returns(Task.FromResult(mockUnusedMaterials));

        mockCommunicationService
            .Setup(x => x.GetUsedStatementsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .Returns(Task.FromResult(mockUsedStatements));

        mockCommunicationService
            .Setup(x => x.GetUsedExhibitsAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .Returns(Task.FromResult(mockUsedExhibits));

        mockCommunicationService
            .Setup(x => x.RetrieveAllAttachmentsAsync(mockCommunications, It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new Exception("Simulated failure for CommunicationId"));

        // Act
        IActionResult result = await getCaseMaterialsFunction.Run(mockRequest.Object, 123);

        // Assert
        ObjectResult objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, objectResult.StatusCode);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseMaterials function processed a request"));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseMaterials function encountered an error fetching attachments for caseId [123]"));
    }

    /// <summary>
    /// Tests that the function correctly handles attachments and maps them to communications.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsUpdatedCommunications_WhenAttachmentsAreRetrieved()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        var mockCommunications = new List<Communication>
        {
            new Communication(1, "FileA.pdf", "Subject A", 1012, 123, "/some/path/doc1.pdf", "Pending", "Administrative", "Type A", false),
            new Communication(2, "FileB.pdf", "Subject B", 1034, 456, "/some/path/doc2.pdf", "Complete", "Evidential", "Type B", false),
        };

        var mockAttachments = new List<Attachment>
        {
            new Attachment(101, "Attachment A", "Description A", "/path/to/attachmentA", "Class A", 5, null, null, null, "Tag A", 123, "AttachmentA.pdf", "UserA", 111, "Processed", "DirectionA"),
            new Attachment(102, "Attachment B", "Description B", "/path/to/attachmentB", "Class B", 10, null, null, null, "Tag B", 456, "AttachmentB.pdf", "UserB", 222, "NotProcessed", "DirectionB"),
        };

        var mockCombindedCommunications = new List<Communication>
        {
            new Communication(1, "FileA.pdf", "Subject A", 1012, 123, "/some/path/doc1.pdf", "Pending", "Administrative", "Type A", false),
            new Communication(2, "FileB.pdf", "Subject B", 1034, 456, "/some/path/doc2.pdf", "Complete", "Evidential", "Type B", false),
            new Communication(0, "AttachmentA.pdf", "Attachment A", 5, 101, "/path/to/attachmentA", "Pending", "Administrative", "Type A", false),
            new Communication(0, "AttachmentB.pdf", "Attachment B", 10, 102, "/path/to/attachmentB", "Complete", "Administrative", "Type B", false),
        };

        var unusedMaterials = new UnusedMaterialsResponse();
        var usedStatements = new Common.Dto.Response.HouseKeeping.UsedStatementsResponse();
        var usedExhibits = new UsedExhibitsResponse();
        var usedMgForms = new UsedMgFormsResponse();
        var usedOtherMaterials = new UsedOtherMaterialsResponse();
        var exhibitProducers = new ExhibitProducersResponse();

        var mockCaseMaterials = new List<CaseMaterial>
        {
            new CaseMaterial(1, "FileA.pdf", "Subject A", 1012, 123, "/some/path/doc1.pdf", "Administrative", "Type A", false, "Pending"),
            new CaseMaterial(2, "FileB.pdf", "Subject B", 1034, 456, "/some/path/doc2.pdf", "Evidential", "Type B", false, "Pending"),
            new CaseMaterial(3, "AttachmentA.pdf", "Attachment A", 5, 101, "/path/to/attachmentA", "Administrative", "Type A", false, "Pending"),
            new CaseMaterial(4, "AttachmentB.pdf", "Attachment B", 10, 102, "/path/to/attachmentB", "Administrative", "Type B", false, "Pending"),
        };

        mockCaseMaterialService
            .Setup(x => x.RetrieveCaseMaterialsAsync(123, It.IsAny<CmsAuthValues>()))
            .ReturnsAsync((mockCommunications, unusedMaterials, usedStatements, usedExhibits, usedMgForms, usedOtherMaterials, exhibitProducers));

        mockCommunicationService
            .Setup(x => x.RetrieveAllAttachmentsAsync(mockCommunications, It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(mockAttachments);

        mockCommunicationService
            .Setup(x => x.MapAttachmentsToCommunications(It.IsAny<List<Attachment>>()))
            .Returns(new List<Communication>
            {
                new Communication(0, "AttachmentA.pdf", "Attachment A", 5, 101, "/path/to/attachmentA", "Pending", "Administrative", "Type A", false),
                new Communication(0, "AttachmentB.pdf", "Attachment B", 10, 102, "/path/to/attachmentB", "Complete", "Administrative", "Type B", false),
            });

        mockCaseMaterialService
            .Setup(x => x.MapCommunicationsToCaseMaterials(mockCombindedCommunications))
            .Returns(mockCaseMaterials);

        // Act
        IActionResult result = await getCaseMaterialsFunction.Run(mockRequest.Object, 123);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result); // Verify we got an OkObjectResult

        Assert.NotNull(okResult.Value);

        List<CaseMaterial> caseMaterials = Assert.IsType<List<CaseMaterial>>(okResult.Value);

        Assert.NotEmpty(caseMaterials);
        Assert.Equal(4, caseMaterials.Count); // Ensure both attachments were mapped
        Assert.Contains(caseMaterials, c => c.OriginalFileName == "FileA.pdf");
        Assert.Contains(caseMaterials, c => c.OriginalFileName == "FileB.pdf");
        Assert.Contains(caseMaterials, c => c.OriginalFileName == "AttachmentA.pdf");
        Assert.Contains(caseMaterials, c => c.OriginalFileName == "AttachmentB.pdf");
        Assert.Contains(caseMaterials, c => c.OriginalFileName == "AttachmentB.pdf");
        Assert.Contains(caseMaterials, c => c.ReadStatus == "Unread");

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseMaterials function processed a request"));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Retrieving attachments for all communications ..."));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [123] GetCaseMaterials function completed"));
    }

    /// <summary>
    /// Tests that the function correctly handles attachments with exhibits and communications and maps them to communications.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsUpdatedCommunications_WhenAttachmentsWithExhibitsAndStatementsAreRetrieved()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        var mockCommunications = new List<Communication>
        {
            new Communication(1, "FileA.pdf", "Subject A", 1012, 123, "/some/path/doc1.pdf", "Pending", "Administrative", "Type A", false),
            new Communication(2, "FileB.pdf", "Subject B", 1034, 456, "/some/path/doc2.pdf", "Complete", "Evidential", "Type B", false),
        };

        // Mock attachments including the required DocumentTypeId
        var mockAttachments = new List<Attachment>
        {
            new Attachment(
                MaterialId: 101,
                Name: "Attachment A",
                Description: "Description A",
                Link: "/path/to/attachmentA",
                Classification: "Class A",
                DocumentTypeId: 5,
                NumOfDocVersions: null,
                Statement: new StatementAttachmentSubType(
                    WitnessName: "John Doe",
                    WitnessTitle: "Detective",
                    WitnessShoulderNo: "12345",
                    StatementNo: "ST101",
                    Date: "2025-01-01",
                    Witness: 1),
                Exhibit: new ExhibitAttachmentSubType(
                    Reference: "Exhibit101",
                    Item: "Item A",
                    Producer: "Producer A"),
                Tag: "Tag A",
                DocId: 123,
                OriginalFileName: "AttachmentA.pdf",
                CheckedOutTo: "UserA",
                DocumentId: 111,
                OcrProcessed: "Processed",
                Direction: "DirectionA"),

            new Attachment(
                MaterialId: 102,
                Name: "Attachment B",
                Description: "Description B",
                Link: "/path/to/attachmentB",
                Classification: "Class B",
                DocumentTypeId: 10,
                NumOfDocVersions: null,
                Statement: new StatementAttachmentSubType(
                    WitnessName: "Jane Smith",
                    WitnessTitle: "Officer",
                    WitnessShoulderNo: "67890",
                    StatementNo: "ST102",
                    Date: "2025-01-02",
                    Witness: 2),
                Exhibit: new ExhibitAttachmentSubType(
                    Reference: "Exhibit102",
                    Item: "Item B",
                    Producer: "Producer B"),
                Tag: "Tag B",
                DocId: 456,
                OriginalFileName: "AttachmentB.pdf",
                CheckedOutTo: "UserB",
                DocumentId: 222,
                OcrProcessed: "NotProcessed",
                Direction: "DirectionB"),
        };

        var mockCombindedCommunications = new List<Communication>
        {
            new Communication(1, "FileA.pdf", "Subject A", 1012, 123, "/some/path/doc1.pdf", "Pending", "Administrative", "Type A", false),
            new Communication(2, "FileB.pdf", "Subject B", 1034, 456, "/some/path/doc2.pdf", "Complete", "Evidential", "Type B", false),
            new Communication(0, "AttachmentA.pdf", "Attachment A", 5, 101, "/path/to/attachmentA", "Pending", "Administrative", "Type A", false),
            new Communication(0, "AttachmentB.pdf", "Attachment B", 10, 102, "/path/to/attachmentB", "Complete", "Administrative", "Type B", false),
        };

        var unusedMaterials = new UnusedMaterialsResponse();
        var usedStatements = new Common.Dto.Response.HouseKeeping.UsedStatementsResponse();
        var usedExhibits = new UsedExhibitsResponse();
        var usedMgForms = new UsedMgFormsResponse();
        var usedOtherMaterials = new UsedOtherMaterialsResponse();
        var exhibitProducers = new ExhibitProducersResponse();

        var mockCaseMaterials = new List<CaseMaterial>
        {
            new CaseMaterial(1, "FileA.pdf", "Subject A", 1012, 123, "/some/path/doc1.pdf", "Administrative", "Type A", false, "Pending"),
            new CaseMaterial(2, "FileB.pdf", "Subject B", 1034, 456, "/some/path/doc2.pdf", "Evidential", "Type B", false, "Complete"),
            new CaseMaterial(3, "AttachmentA.pdf", "Attachment A", 5, 101, "/path/to/attachmentA", "Administrative", "Type A", false, "Pending"),
            new CaseMaterial(4, "AttachmentB.pdf", "Attachment B", 10, 102, "/path/to/attachmentB", "Administrative", "Type B", false, "Complete"),
        };

        mockCaseMaterialService
            .Setup(x => x.RetrieveCaseMaterialsAsync(123, It.IsAny<CmsAuthValues>()))
            .ReturnsAsync((mockCommunications, unusedMaterials, usedStatements, usedExhibits, usedMgForms, usedOtherMaterials, exhibitProducers));

        mockCommunicationService
            .Setup(x => x.RetrieveAllAttachmentsAsync(mockCommunications, It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(mockAttachments);

        mockCommunicationService
            .Setup(x => x.MapAttachmentsToCommunications(It.IsAny<List<Attachment>>()))
            .Returns(new List<Communication>
            {
                new Communication(0, "AttachmentA.pdf", "Attachment A", 5, 101, "/path/to/attachmentA", "Pending", "Administrative", "Type A", false),
                new Communication(0, "AttachmentB.pdf", "Attachment B", 10, 102, "/path/to/attachmentB", "Complete", "Administrative", "Type B", false),
            });

        mockCaseMaterialService
            .Setup(x => x.MapCommunicationsToCaseMaterials(mockCombindedCommunications))
            .Returns(mockCaseMaterials);

        // Act
        IActionResult result = await getCaseMaterialsFunction.Run(mockRequest.Object, 123);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result); // Verify we got an OkObjectResult

        Assert.NotNull(okResult.Value);

        List<CaseMaterial> caseMaterials = Assert.IsType<List<CaseMaterial>>(okResult.Value);

        Assert.NotEmpty(caseMaterials);
        Assert.Equal(4, caseMaterials.Count); // Ensure both attachments were mapped
        Assert.Contains(caseMaterials, c => c.OriginalFileName == "FileA.pdf");
        Assert.Contains(caseMaterials, c => c.OriginalFileName == "FileB.pdf");
        Assert.Contains(caseMaterials, c => c.OriginalFileName == "AttachmentA.pdf");
        Assert.Contains(caseMaterials, c => c.OriginalFileName == "AttachmentB.pdf");

        Assert.Equal("Unread", caseMaterials[0].ReadStatus);
        Assert.Equal("Read", caseMaterials[1].ReadStatus);
        Assert.Equal("Unread", caseMaterials[2].ReadStatus);
        Assert.Equal("Read", caseMaterials[3].ReadStatus);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} GetCaseMaterials function processed a request"));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} caseId [123] material count:"));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Retrieving attachments for all communications ..."));

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [123] GetCaseMaterials function completed"));
    }

    /// <summary>
    /// Tests that the function correctly handles and maps Used MG Forms when they are present.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsOkResult_WhenUsedMgFormsArePresent()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        var mockCommunications = new List<Communication>
        {
            new Communication(1, "FileA.pdf", "Subject A", 1012, 123, "/some/path/doc1.pdf", "None", "Administrative", "Type A", false),
        };

        var mockCaseMaterials = new List<CaseMaterial>
        {
            new CaseMaterial(1, "FileA.pdf", "Subject A", 1012, 123, "/some/path/doc1.pdf", "Administrative", "Type A", false, "None"),
        };

        // Create Used MG Forms data
        var usedMgForm1 = new MgForm(
            Id: 1,
            Title: "Used MG Form One",
            OriginalFileName: "usedMgForm1.pdf",
            MaterialType: "1202",
            DocumentType: 2,
            Link: "http://example1.com",
            Status: "Used",
            Date: receivedDate);

        var usedMgForm2 = new MgForm(
            Id: 2,
            Title: "Used MG Form Two",
            OriginalFileName: "usedMgForm2.pdf",
            MaterialType: "1203",
            DocumentType: 3,
            Link: "http://example2.com",
            Status: "Used",
            Date: receivedDate);

        var usedMgForms = new UsedMgFormsResponse
        {
            MgForms = new List<MgForm> { usedMgForm1, usedMgForm2 },
        };

        var mappedUsedMgFormsCaseMaterials = new List<CaseMaterial>
        {
            new CaseMaterial(1, "usedMgForm1.pdf", "Used MG Form One", 1202, 1, "http://example1.com", "MG Form", "MG Form", false, "Used"),
            new CaseMaterial(2, "usedMgForm2.pdf", "Used MG Form Two", 1203, 2, "http://example2.com", "MG Form", "MG Form", false, "Used"),
        };

        // Mock empty responses for other materials
        var unusedMaterials = new UnusedMaterialsResponse();
        var usedStatements = new Common.Dto.Response.HouseKeeping.UsedStatementsResponse();
        var usedExhibits = new UsedExhibitsResponse();
        var usedOtherMaterials = new UsedOtherMaterialsResponse();
        var exhibitProducers = new ExhibitProducersResponse();

        // Set up service mocks
        mockCaseMaterialService
            .Setup(x => x.RetrieveCaseMaterialsAsync(123, It.IsAny<CmsAuthValues>()))
            .ReturnsAsync((mockCommunications, unusedMaterials, usedStatements, usedExhibits, usedMgForms, usedOtherMaterials, exhibitProducers));

        mockCaseMaterialService
            .Setup(x => x.MapCommunicationsToCaseMaterials(mockCommunications))
            .Returns(mockCaseMaterials);

        mockCaseMaterialService
            .Setup(x => x.MapUsedMgFormsToCaseMaterials(usedMgForms))
            .Returns(mappedUsedMgFormsCaseMaterials);

        // Act
        IActionResult result = await getCaseMaterialsFunction.Run(mockRequest.Object, 123);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        List<CaseMaterial> caseMaterials = Assert.IsType<List<CaseMaterial>>(okResult.Value);
        Assert.NotEmpty(caseMaterials);

        // Verify that MapUsedMgFormsToCaseMaterials was called
        mockCaseMaterialService.Verify(
            x => x.MapUsedMgFormsToCaseMaterials(usedMgForms),
            Times.Once);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [123] GetCaseMaterials function completed"));
    }

    /// <summary>
    /// Tests that the function correctly handles and maps Used Other Materials when they are present.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsOkResult_WhenUsedOtherMaterialsArePresent()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        var mockCommunications = new List<Communication>
        {
            new Communication(1, "FileA.pdf", "Subject A", 1012, 123, "/some/path/doc1.pdf", "None", "Administrative", "Type A", false),
        };

        var mockCaseMaterials = new List<CaseMaterial>
        {
            new CaseMaterial(1, "FileA.pdf", "Subject A", 1012, 123, "/some/path/doc1.pdf", "Administrative", "Type A", false, "None"),
        };

        // Create Used Other Materials data
        var usedOtherMaterial1 = new MgForm(
            Id: 1,
            Title: "Used Other Material One",
            OriginalFileName: "usedOtherMaterial1.pdf",
            MaterialType: "1204",
            DocumentType: 2,
            Link: "http://example1.com",
            Status: "Used",
            Date: receivedDate);

        var usedOtherMaterial2 = new MgForm(
            Id: 2,
            Title: "Used Other Material Two",
            OriginalFileName: "usedOtherMaterial2.pdf",
            MaterialType: "1205",
            DocumentType: 3,
            Link: "http://example2.com",
            Status: "Used",
            Date: receivedDate);

        var usedOtherMaterials = new UsedOtherMaterialsResponse
        {
            MgForms = new List<MgForm> { usedOtherMaterial1, usedOtherMaterial2 },
        };

        var mappedUsedOtherMaterialsCaseMaterials = new List<CaseMaterial>
        {
            new CaseMaterial(1, "usedOtherMaterial1.pdf", "Used Other Material One", 1204, 1, "http://example1.com", "Other Material", "Other Material", false, "Used"),
            new CaseMaterial(2, "usedOtherMaterial2.pdf", "Used Other Material Two", 1205, 2, "http://example2.com", "Other Material", "Other Material", false, "Used"),
        };

        // Mock empty responses for other materials
        var unusedMaterials = new UnusedMaterialsResponse();
        var usedStatements = new Common.Dto.Response.HouseKeeping.UsedStatementsResponse();
        var usedExhibits = new UsedExhibitsResponse();
        var usedMgForms = new UsedMgFormsResponse();
        var exhibitProducers = new ExhibitProducersResponse();

        // Set up service mocks
        mockCaseMaterialService
            .Setup(x => x.RetrieveCaseMaterialsAsync(123, It.IsAny<CmsAuthValues>()))
            .ReturnsAsync((mockCommunications, unusedMaterials, usedStatements, usedExhibits, usedMgForms, usedOtherMaterials, exhibitProducers));

        mockCaseMaterialService
            .Setup(x => x.MapCommunicationsToCaseMaterials(mockCommunications))
            .Returns(mockCaseMaterials);

        mockCaseMaterialService
            .Setup(x => x.MapUsedOtherMaterialsToCaseMaterials(usedOtherMaterials))
            .Returns(mappedUsedOtherMaterialsCaseMaterials);

        // Act
        IActionResult result = await getCaseMaterialsFunction.Run(mockRequest.Object, 123);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        List<CaseMaterial> caseMaterials = Assert.IsType<List<CaseMaterial>>(okResult.Value);
        Assert.NotEmpty(caseMaterials);

        // Verify that MapUsedOtherMaterialsToCaseMaterials was called
        mockCaseMaterialService.Verify(
            x => x.MapUsedOtherMaterialsToCaseMaterials(usedOtherMaterials),
            Times.Once);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [123] GetCaseMaterials function completed"));
    }

    /// <summary>
    /// Tests that the function correctly handles both Used MG Forms and Used Other Materials when both are present.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsOkResult_WhenBothUsedMgFormsAndUsedOtherMaterialsArePresent()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        var mockCommunications = new List<Communication>
        {
            new Communication(1, "FileA.pdf", "Subject A", 1012, 123, "/some/path/doc1.pdf", "None", "Administrative", "Type A", false),
        };

        var mockCaseMaterials = new List<CaseMaterial>
        {
            new CaseMaterial(1, "FileA.pdf", "Subject A", 1012, 123, "/some/path/doc1.pdf", "Administrative", "Type A", false, "None"),
        };

        // Create Used MG Forms data
        var usedMgForm = new MgForm(
            Id: 1,
            Title: "Used MG Form",
            OriginalFileName: "usedMgForm.pdf",
            MaterialType: "1202",
            DocumentType: 2,
            Link: "http://example1.com",
            Status: "Used",
            Date: receivedDate);

        var usedMgForms = new UsedMgFormsResponse
        {
            MgForms = new List<MgForm> { usedMgForm },
        };

        // Create Used Other Materials data
        var usedOtherMaterial = new MgForm(
            Id: 2,
            Title: "Used Other Material",
            OriginalFileName: "usedOtherMaterial.pdf",
            MaterialType: "1204",
            DocumentType: 3,
            Link: "http://example2.com",
            Status: "Used",
            Date: receivedDate);

        var usedOtherMaterials = new UsedOtherMaterialsResponse
        {
            MgForms = new List<MgForm> { usedOtherMaterial },
        };

        var mappedUsedMgFormsCaseMaterials = new List<CaseMaterial>
        {
            new CaseMaterial(1, "usedMgForm.pdf", "Used MG Form", 1202, 1, "http://example1.com", "MG Form", "MG Form", false, "Used"),
        };

        var mappedUsedOtherMaterialsCaseMaterials = new List<CaseMaterial>
        {
            new CaseMaterial(2, "usedOtherMaterial.pdf", "Used Other Material", 1204, 2, "http://example2.com", "Other Material", "Other Material", false, "Used"),
        };

        // Mock empty responses for other materials
        var unusedMaterials = new UnusedMaterialsResponse();
        var usedStatements = new Common.Dto.Response.HouseKeeping.UsedStatementsResponse();
        var usedExhibits = new UsedExhibitsResponse();
        var exhibitProducers = new ExhibitProducersResponse();

        // Set up service mocks
        mockCaseMaterialService
            .Setup(x => x.RetrieveCaseMaterialsAsync(123, It.IsAny<CmsAuthValues>()))
            .ReturnsAsync((mockCommunications, unusedMaterials, usedStatements, usedExhibits, usedMgForms, usedOtherMaterials, exhibitProducers));

        mockCaseMaterialService
            .Setup(x => x.MapCommunicationsToCaseMaterials(mockCommunications))
            .Returns(mockCaseMaterials);

        mockCaseMaterialService
            .Setup(x => x.MapUsedMgFormsToCaseMaterials(usedMgForms))
            .Returns(mappedUsedMgFormsCaseMaterials);

        mockCaseMaterialService
            .Setup(x => x.MapUsedOtherMaterialsToCaseMaterials(usedOtherMaterials))
            .Returns(mappedUsedOtherMaterialsCaseMaterials);

        // Act
        IActionResult result = await getCaseMaterialsFunction.Run(mockRequest.Object, 123);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        List<CaseMaterial> caseMaterials = Assert.IsType<List<CaseMaterial>>(okResult.Value);
        Assert.NotEmpty(caseMaterials);

        // Verify that both mapping methods were called
        mockCaseMaterialService.Verify(
            x => x.MapUsedMgFormsToCaseMaterials(usedMgForms),
            Times.Once);

        mockCaseMaterialService.Verify(
            x => x.MapUsedOtherMaterialsToCaseMaterials(usedOtherMaterials),
            Times.Once);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [123] GetCaseMaterials function completed"));
    }

    /// <summary>
    /// Tests that the function handles empty Used MG Forms collections gracefully.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Run_ReturnsOkResult_WhenUsedMgFormsAreEmpty()
    {
        // Arrange
        var mockRequest = SetUpMockRequest();

        var mockCommunications = new List<Communication>
        {
            new Communication(1, "FileA.pdf", "Subject A", 1012, 123, "/some/path/doc1.pdf", "None", "Administrative", "Type A", false),
        };

        var mockCaseMaterials = new List<CaseMaterial>
        {
            new CaseMaterial(1, "FileA.pdf", "Subject A", 1012, 123, "/some/path/doc1.pdf", "Administrative", "Type A", false, "None"),
        };

        // Create empty Used MG Forms
        var usedMgForms = new UsedMgFormsResponse
        {
            MgForms = new List<MgForm>(),
        };

        // Mock empty responses for other materials
        var unusedMaterials = new UnusedMaterialsResponse();
        var usedStatements = new Common.Dto.Response.HouseKeeping.UsedStatementsResponse();
        var usedExhibits = new UsedExhibitsResponse();
        var usedOtherMaterials = new UsedOtherMaterialsResponse();
        var exhibitProducers = new ExhibitProducersResponse();

        // Set up service mocks
        mockCaseMaterialService
            .Setup(x => x.RetrieveCaseMaterialsAsync(123, It.IsAny<CmsAuthValues>()))
            .ReturnsAsync((mockCommunications, unusedMaterials, usedStatements, usedExhibits, usedMgForms, usedOtherMaterials, exhibitProducers));

        mockCaseMaterialService
            .Setup(x => x.MapCommunicationsToCaseMaterials(mockCommunications))
            .Returns(mockCaseMaterials);

        // Act
        IActionResult result = await getCaseMaterialsFunction.Run(mockRequest.Object, 123);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        // Verify that MapUsedMgFormsToCaseMaterials was NOT called because the collection is empty
        mockCaseMaterialService.Verify(
            x => x.MapUsedMgFormsToCaseMaterials(It.IsAny<UsedMgFormsResponse>()),
            Times.Never);

        Assert.Contains(mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [123] GetCaseMaterials function completed"));
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
