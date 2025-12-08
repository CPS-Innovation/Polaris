// <copyright file="MaterialReclassificationOrchestrationServiceUnitTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services.Tests;

using Common.Constants;
using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.HouseKeeping;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for MaterialReclassificationOrchestrationService.
/// </summary>
public class MaterialReclassificationOrchestrationServiceUnitTests
{
    private readonly TestLogger<MaterialReclassificationOrchestrationService> mockLogger;
    private readonly Mock<IReclassificationService> mockReclassificationService;
    private readonly Mock<ICommunicationService> mockCommunicationService;
    private readonly Mock<ICaseActionPlanService> mockActionPlanService;
    private readonly Mock<IWitnessService> mockWitnessService;
    private readonly MaterialReclassificationOrchestrationService sutOrchestrationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialReclassificationOrchestrationServiceUnitTests"/> class.
    /// </summary>
    public MaterialReclassificationOrchestrationServiceUnitTests()
    {
        this.mockLogger = new TestLogger<MaterialReclassificationOrchestrationService>();
        this.mockReclassificationService = new Mock<IReclassificationService>();
        this.mockCommunicationService = new Mock<ICommunicationService>();
        this.mockActionPlanService = new Mock<ICaseActionPlanService>();
        this.mockWitnessService = new Mock<IWitnessService>();

        this.sutOrchestrationService = new MaterialReclassificationOrchestrationService(
            this.mockLogger,
            this.mockReclassificationService.Object,
            this.mockCommunicationService.Object,
            this.mockActionPlanService.Object,
            this.mockWitnessService.Object);
    }

    /// <summary>
    /// Tests CompleteReclassificationAsync method invokes operations (Reclassify, AddWitness and AddActionPlan) successful, and returns 'success' operations results.
    /// </summary>
    /// <returns>Asynchronous operation.</returns>
    [Fact]
    public async Task CompleteReclassificationAsync_ReclassifyingMaterialToStatement_AddsNewWitnessAndActionPlan_AllExpectedOperations_Succeed()
    {
        // Arrange
        int caseId = 232;
        int materialId = 123;
        int newWitnessId = 765;
        var cmsAuthValues = new CmsAuthValues("valid cookies", "valid token", Guid.NewGuid());
        var transactionId = Guid.NewGuid();

        var reclassifyCaseMaterialRequest = CreateMockReclassifyToStatmentRequest();
        var addCaseActionPlanRequest = CreateMockAddCaseActionPlanRequest();
        var addWitnessRequest = new WitnessRequest(caseId, null, "Firstname", "Surname");
        var renameMaterialRequest = new RenameMaterialRequest(transactionId, materialId, reclassifyCaseMaterialRequest.subject);

        var completeReclassificationRequest = new CompleteReclassificationRequest(
            reclassifyCaseMaterialRequest,
            addCaseActionPlanRequest,
            addWitnessRequest);

        this.mockWitnessService
            .Setup(x => x.AddWitnessAsync(It.IsAny<string>(), caseId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ReturnsAsync(newWitnessId);

        this.mockActionPlanService
            .Setup(x => x.AddCaseActionPlanAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<AddCaseActionPlanRequest>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(new NoContentResult());

        this.mockReclassificationService
            .Setup(x => x.ReclassifyCaseMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<ReclassifyStatementRequest>(), It.IsAny<ReclassifyExhibitRequest>(), It.IsAny<Guid>()))
            .ReturnsAsync(It.IsAny<ReclassificationResponse>());

        this.mockCommunicationService
            .Setup(x => x.RenameMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ReturnsAsync(It.IsAny<Common.Dto.Response.HouseKeeping.RenameMaterialResponse>());

        // Act
        var result = await this.sutOrchestrationService.CompleteReclassificationAsync(
            caseId,
            materialId,
            It.IsAny<CmsAuthValues>(),
            completeReclassificationRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(typeof(CompleteReclassificationResponse), result.GetType());

        Assert.True(result.overallSuccess);
        Assert.Equal("Success", result.status);

        Assert.NotNull(result.addWitnessResult);
        Assert.Equal("AddWitness", result.addWitnessResult.OperationName);
        Assert.True(result.addWitnessResult.Success);
        Assert.Null(result.addWitnessResult.ErrorMessage);

        Assert.NotNull(result.reclassificationResult);
        Assert.Equal("ReclassifyCaseMaterial", result.reclassificationResult.OperationName);
        Assert.True(result.reclassificationResult.Success);
        Assert.Null(result.reclassificationResult.ErrorMessage);

        Assert.NotNull(result.actionPlanResult);
        Assert.Equal("AddCaseActionPlan", result.actionPlanResult.OperationName);
        Assert.True(result.actionPlanResult.Success);
        Assert.Null(result.actionPlanResult.ErrorMessage);

        // Rename is not called when reclassifying to STATEMENT.
        Assert.Null(result.renameMaterialResult);

        // Assert logs
        Assert.Contains(this.mockLogger.Logs, log =>
         log.LogLevel == LogLevel.Information &&
         log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} starting complete reclassification for MaterialId: [{materialId}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
         log.LogLevel == LogLevel.Information &&
         log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} executing witness addition for CaseId: [{caseId}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
        log.LogLevel == LogLevel.Information &&
        log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} executing action plan for URN: [{reclassifyCaseMaterialRequest.urn}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
         log.LogLevel == LogLevel.Information &&
        log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} executing reclassification for MaterialId: [{materialId}]"));

        this.mockWitnessService.Verify(x => x.AddWitnessAsync(It.IsAny<string>(), caseId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()), Times.Once());
        this.mockReclassificationService.Verify(x => x.ReclassifyCaseMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<ReclassifyStatementRequest>(), It.IsAny<ReclassifyExhibitRequest>(), It.IsAny<Guid>()), Times.Once());
        this.mockActionPlanService.Verify(x => x.AddCaseActionPlanAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<AddCaseActionPlanRequest>(), It.IsAny<CmsAuthValues>()), Times.Once());

        this.mockCommunicationService.Verify(x => x.RenameMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()), Times.Never());
    }

    /// <summary>
    /// Tests CompleteReclassificationAsync method invokes operations (Reclassify, AddWitness and AddActionPlan), and returns a parital success operations results.
    /// </summary>
    /// <returns>Asynchronous operation.</returns>
    [Fact]
    public async Task CompleteReclassificationAsync_ReclassifyingMaterialToStatement_AddingNewWitnessAndActionPlan_ExecutesExpectedOperations_ParitalSuccess()
    {
        // Arrange
        int caseId = 232;
        int materialId = 123;
        int newWitnessId = 765;
        var cmsAuthValues = new CmsAuthValues("valid cookies", "valid token", Guid.NewGuid());
        var transactionId = Guid.NewGuid();

        var reclassifyCaseMaterialRequest = CreateMockReclassifyToStatmentRequest();
        var addCaseActionPlanRequest = CreateMockAddCaseActionPlanRequest();
        var addWitnessRequest = new WitnessRequest(caseId, null, "Firstname", "Surname");
        var renameMaterialRequest = new RenameMaterialRequest(transactionId, materialId, reclassifyCaseMaterialRequest.subject);

        var completeReclassificationRequest = new CompleteReclassificationRequest(
            reclassifyCaseMaterialRequest,
            addCaseActionPlanRequest,
            addWitnessRequest);

        this.mockWitnessService
            .Setup(x => x.AddWitnessAsync(It.IsAny<string>(), caseId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ReturnsAsync(newWitnessId);

        var exception = new Exception("DDEI API error");

        // Simulate that action plan execution failed.
        this.mockActionPlanService
            .Setup(x => x.AddCaseActionPlanAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<AddCaseActionPlanRequest>(), It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(exception);

        this.mockReclassificationService
            .Setup(x => x.ReclassifyCaseMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<ReclassifyStatementRequest>(), It.IsAny<ReclassifyExhibitRequest>(), It.IsAny<Guid>()))
            .ReturnsAsync(It.IsAny<ReclassificationResponse>());

        this.mockCommunicationService
            .Setup(x => x.RenameMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ReturnsAsync(It.IsAny<Common.Dto.Response.HouseKeeping.RenameMaterialResponse>());

        // Act
        var result = await this.sutOrchestrationService.CompleteReclassificationAsync(
            caseId,
            materialId,
            It.IsAny<CmsAuthValues>(),
            completeReclassificationRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(typeof(CompleteReclassificationResponse), result.GetType());

        Assert.False(result.overallSuccess);
        Assert.Equal("PartialSuccess", result.status);

        Assert.NotNull(result.addWitnessResult);
        Assert.Equal("AddWitness", result.addWitnessResult.OperationName);
        Assert.True(result.addWitnessResult.Success);
        Assert.Null(result.addWitnessResult.ErrorMessage);

        Assert.NotNull(result.reclassificationResult);
        Assert.Equal("ReclassifyCaseMaterial", result.reclassificationResult.OperationName);
        Assert.True(result.reclassificationResult.Success);
        Assert.Null(result.reclassificationResult.ErrorMessage);

        Assert.NotNull(result.actionPlanResult);
        Assert.Equal("AddCaseActionPlan", result.actionPlanResult.OperationName);
        Assert.False(result.actionPlanResult.Success);
        Assert.Equal(exception.Message, result.actionPlanResult.ErrorMessage);

        // Rename is not called when reclassifying to STATEMENT.
        Assert.Null(result.renameMaterialResult);

        // Assert logs
        Assert.Contains(this.mockLogger.Logs, log =>
         log.LogLevel == LogLevel.Information &&
         log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} executing witness addition for CaseId: [{caseId}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
        log.LogLevel == LogLevel.Information &&
        log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} executing action plan for URN: [{reclassifyCaseMaterialRequest.urn}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
         log.LogLevel == LogLevel.Information &&
        log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} executing reclassification for MaterialId: [{materialId}]"));

        this.mockWitnessService.Verify(x => x.AddWitnessAsync(It.IsAny<string>(), caseId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()), Times.Once());
        this.mockReclassificationService.Verify(x => x.ReclassifyCaseMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<ReclassifyStatementRequest>(), It.IsAny<ReclassifyExhibitRequest>(), It.IsAny<Guid>()), Times.Once());
        this.mockActionPlanService.Verify(x => x.AddCaseActionPlanAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<AddCaseActionPlanRequest>(), It.IsAny<CmsAuthValues>()), Times.Once());

        this.mockCommunicationService.Verify(x => x.RenameMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()), Times.Never());
    }

    /// <summary>
    /// Tests CompleteReclassificationAsync method invokes operations (Reclassify, AddWitness and AddActionPlan), all operations fail, and returns a failed operation results.
    /// </summary>
    /// <returns>Asynchronous operation.</returns>
    [Fact]
    public async Task CompleteReclassificationAsync_ReclassifyingMaterialToStatement_AddingNewWitnessAndActionPlan_ExecutesExpectedOperations_Failed()
    {
        // Arrange
        int caseId = 232;
        int materialId = 123;
        var cmsAuthValues = new CmsAuthValues("valid cookies", "valid token", Guid.NewGuid());
        var transactionId = Guid.NewGuid();

        var reclassifyCaseMaterialRequest = CreateMockReclassifyToStatmentRequest();
        var addCaseActionPlanRequest = CreateMockAddCaseActionPlanRequest();
        var addWitnessRequest = new WitnessRequest(caseId, null, "Firstname", "Surname");
        var renameMaterialRequest = new RenameMaterialRequest(transactionId, materialId, reclassifyCaseMaterialRequest.subject);

        var completeReclassificationRequest = new CompleteReclassificationRequest(
            reclassifyCaseMaterialRequest,
            addCaseActionPlanRequest,
            addWitnessRequest);

        var exception = new Exception("DDEI API error");

        this.mockWitnessService
            .Setup(x => x.AddWitnessAsync(It.IsAny<string>(), caseId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ThrowsAsync(exception);

        // Simulate that action plan execution failed.
        this.mockActionPlanService
            .Setup(x => x.AddCaseActionPlanAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<AddCaseActionPlanRequest>(), It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(exception);

        this.mockReclassificationService
            .Setup(x => x.ReclassifyCaseMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<ReclassifyStatementRequest>(), It.IsAny<ReclassifyExhibitRequest>(), It.IsAny<Guid>()))
           .ThrowsAsync(exception);

        this.mockCommunicationService
            .Setup(x => x.RenameMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ThrowsAsync(exception);

        // Act
        var result = await this.sutOrchestrationService.CompleteReclassificationAsync(
            caseId,
            materialId,
            It.IsAny<CmsAuthValues>(),
            completeReclassificationRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(typeof(CompleteReclassificationResponse), result.GetType());

        Assert.False(result.overallSuccess);
        Assert.Equal("Failed", result.status);

        Assert.NotNull(result.addWitnessResult);
        Assert.Equal("AddWitness", result.addWitnessResult.OperationName);
        Assert.False(result.addWitnessResult.Success);
        Assert.Equal(exception.Message, result.addWitnessResult.ErrorMessage);

        Assert.NotNull(result.reclassificationResult);
        Assert.Equal("ReclassifyCaseMaterial", result.reclassificationResult.OperationName);
        Assert.False(result.reclassificationResult.Success);
        Assert.Equal(exception.Message, result.reclassificationResult.ErrorMessage);

        Assert.NotNull(result.actionPlanResult);
        Assert.Equal("AddCaseActionPlan", result.actionPlanResult.OperationName);
        Assert.False(result.actionPlanResult.Success);
        Assert.Equal(exception.Message, result.actionPlanResult.ErrorMessage);

        // Rename is not called when reclassifying to STATEMENT.
        Assert.Null(result.renameMaterialResult);

        // Assert logs
        Assert.Contains(this.mockLogger.Logs, log =>
         log.LogLevel == LogLevel.Error &&
         log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} witness addition failed for CaseId: [{caseId}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
        log.LogLevel == LogLevel.Error &&
        log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Action plan creation failed for URN: [{reclassifyCaseMaterialRequest.urn}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
         log.LogLevel == LogLevel.Error &&
        log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} reclassification failed for MaterialId: [{materialId}]"));

        this.mockWitnessService.Verify(x => x.AddWitnessAsync(It.IsAny<string>(), caseId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()), Times.Once());
        this.mockReclassificationService.Verify(x => x.ReclassifyCaseMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<ReclassifyStatementRequest>(), It.IsAny<ReclassifyExhibitRequest>(), It.IsAny<Guid>()), Times.Once());
        this.mockActionPlanService.Verify(x => x.AddCaseActionPlanAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<AddCaseActionPlanRequest>(), It.IsAny<CmsAuthValues>()), Times.Once());

        this.mockCommunicationService.Verify(x => x.RenameMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()), Times.Never());
    }

    /// <summary>
    /// Tests CompleteReclassificationAsync method invokes operations (Reclassify, Rename) successful, and returns 'success' operations results.
    /// </summary>
    /// <returns>Asynchronous operation.</returns>
    [Fact]
    public async Task CompleteReclassificationAsync_ReclassifyingMaterialToOther_AllExpectedOperations_Succeed()
    {
        // Arrange
        int caseId = 232;
        int materialId = 123;
        int newWitnessId = 765;
        var cmsAuthValues = new CmsAuthValues("valid cookies", "valid token", Guid.NewGuid());
        var transactionId = Guid.NewGuid();

        var reclassifyCaseMaterialRequest = CreateMockReclassifyToOtherRequest();
        var addCaseActionPlanRequest = CreateMockAddCaseActionPlanRequest();
        var addWitnessRequest = new WitnessRequest(caseId, newWitnessId, string.Empty, string.Empty);
        var renameMaterialRequest = new RenameMaterialRequest(transactionId, materialId, reclassifyCaseMaterialRequest.subject);

        var completeReclassificationRequest = new CompleteReclassificationRequest(
            reclassifyCaseMaterialRequest,
            addCaseActionPlanRequest,
            addWitnessRequest);

        this.mockWitnessService
            .Setup(x => x.AddWitnessAsync(It.IsAny<string>(), caseId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ReturnsAsync(newWitnessId);

        this.mockActionPlanService
            .Setup(x => x.AddCaseActionPlanAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<AddCaseActionPlanRequest>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(new NoContentResult());

        this.mockReclassificationService
            .Setup(x => x.ReclassifyCaseMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<ReclassifyStatementRequest>(), It.IsAny<ReclassifyExhibitRequest>(), It.IsAny<Guid>()))
            .ReturnsAsync(It.IsAny<ReclassificationResponse>());

        this.mockCommunicationService
            .Setup(x => x.RenameMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()))
            .ReturnsAsync(It.IsAny<RenameMaterialResponse>());

        // Act
        var result = await this.sutOrchestrationService.CompleteReclassificationAsync(
            caseId,
            materialId,
            It.IsAny<CmsAuthValues>(),
            completeReclassificationRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(typeof(CompleteReclassificationResponse), result.GetType());
        Assert.True(result.overallSuccess);
        Assert.Equal("Success", result.status);

        // Witness addition not called
        Assert.Null(result.addWitnessResult);

        Assert.NotNull(result.reclassificationResult);
        Assert.Equal("ReclassifyCaseMaterial", result.reclassificationResult.OperationName);
        Assert.True(result.reclassificationResult.Success);
        Assert.Null(result.reclassificationResult.ErrorMessage);

        // Rename material called for not statement reclassification.
        Assert.NotNull(result.renameMaterialResult);
        Assert.Equal("RenameMaterial", result.renameMaterialResult.OperationName);
        Assert.True(result.renameMaterialResult.Success);
        Assert.Null(result.renameMaterialResult.ErrorMessage);

        // Assert logs
        Assert.Contains(this.mockLogger.Logs, log =>
         log.LogLevel == LogLevel.Information &&
         log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} starting complete reclassification for MaterialId: [{materialId}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
         log.LogLevel == LogLevel.Information &&
        log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} executing reclassification for MaterialId: [{materialId}]"));

        this.mockWitnessService.Verify(x => x.AddWitnessAsync(It.IsAny<string>(), caseId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()), Times.Never());
        this.mockReclassificationService.Verify(x => x.ReclassifyCaseMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<ReclassifyStatementRequest>(), It.IsAny<ReclassifyExhibitRequest>(), It.IsAny<Guid>()), Times.Once());
        this.mockActionPlanService.Verify(x => x.AddCaseActionPlanAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<AddCaseActionPlanRequest>(), It.IsAny<CmsAuthValues>()), Times.Never());

        this.mockCommunicationService.Verify(x => x.RenameMaterialAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()), Times.Once());
    }

    private static AddCaseActionPlanRequest CreateMockAddCaseActionPlanRequest()
    {
        return new AddCaseActionPlanRequest(
            urn: "1234567890",
            fullDefendantName: "SURNAME, Firstname",
            allDefendants: false,
            date: new DateOnly(2025, 5, 27),
            dateExpected: null,
            dateTimeCreated: new DateTime(2025, 5, 27),
            type: "StartFileBuild",
            actionPointText: "Action point text",
            status: null,
            statusDescription: null,
            dG6Justification: null,
            createdByOrganisation: "Cps",
            expectedDateUpdated: false,
            partyType: null,
            policeChangeReason: null,
            statusUpdated: false,
            syncedWithPolice: null,
            cpsChangeReason: null,
            duplicateOriginalMaterial: null,
            material: null,
            chaserTaskDate: new DateOnly(2025, 6, 10),
            defendantId: 2786607,
            steps: [new Step("KWD", "Key Witness Details", "Step text", false, false)]);
    }

    private static ReclassifyCaseMaterialRequest CreateMockReclassifyToStatmentRequest()
    {
        return new ReclassifyCaseMaterialRequest(
            "1212122",
            "STATEMENT",
            123,
            432,
            true,
            "this is a test subject",
            new ReclassifyStatementRequest(),
            exhibitRequest: null);
    }

    private static ReclassifyCaseMaterialRequest CreateMockReclassifyToOtherRequest()
    {
        return new ReclassifyCaseMaterialRequest(
            "1212122",
            "OTHER",
            123,
            432,
            true,
            "this is a test subject",
            null,
            exhibitRequest: null);
    }
}
