// <copyright file="CompleteReclassificationTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions.HouseKeeping;

using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Constants;
using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.HouseKeeping;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using Cps.Fct.Hk.Ui.Services.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Functions.HouseKeeping;
using Xunit;

/// <summary>
/// Represents unit tests for CompleteReclassification.
/// </summary>
public class CompleteReclassificationTests
{
    private readonly TestLogger<CompleteReclassification> mockLogger;
    private readonly Mock<IMaterialReclassificationOrchestrationService> mockOrchestrationService;
    private readonly CompleteReclassification sutCompleteReclassification;
    private readonly CompleteReclassificationRequestValidator validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompleteReclassificationTests"/> class.
    /// </summary>
    public CompleteReclassificationTests()
    {
        this.mockLogger = new TestLogger<CompleteReclassification>();
        this.mockOrchestrationService = new Mock<IMaterialReclassificationOrchestrationService>();
        this.validator = new CompleteReclassificationRequestValidator();

        this.sutCompleteReclassification = new CompleteReclassification(
            this.mockLogger,
            this.mockOrchestrationService.Object,
            this.validator);
    }

    /// <summary>
    /// Tests CompleteReclassificationAsync when all expected reclassification operations succeed.
    /// </summary>
    /// <returns> Asynchronous unit test result.</returns>
    [Fact]
    public async Task Run_ReturnsOkResultWithOperationsResultobject_WhenAllOperationsSucceed()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";
        int caseId = 232;
        int materialId = 123;
        int newWitnessId = 765;
        var cmsAuthValues = new CmsAuthValues("valid cookies", "valid token", Guid.NewGuid());
        var transactionId = Guid.NewGuid();

        var reclassifyCaseMaterialRequest = CreateMockReclassifyToOtherRequest();
        var addCaseActionPlanRequest = CreateMockAddCaseActionPlanRequest();
        var addWitnessRequest = new WitnessRequest(caseId, newWitnessId, string.Empty, string.Empty);

        var completeReclassificationRequest = new CompleteReclassificationRequest(
            reclassifyCaseMaterialRequest,
            addCaseActionPlanRequest,
            addWitnessRequest);

        string httpRequestString = JsonSerializer.Serialize(completeReclassificationRequest);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);

        httpRequest.Body = stream;

        // Expected reponse containing all operation results.
        var expectedResponse = new CompleteReclassificationResponse(
            overallSuccess: true,
            status: "Success",
            materialId: materialId,
            transactionId: transactionId.ToString(),
            new(true, "ReclassifyCaseMaterial", null, 12121),
            renameMaterialResult: null,
            new(true, "AddCaseActionPlan", null, null),
            new(true, "AddWitness", null, 543),
            errors: []);

        // Mock the material occhestration service to return the expected response.
        this.mockOrchestrationService
            .Setup(x => x.CompleteReclassificationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CompleteReclassificationRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        IActionResult result = await this.sutCompleteReclassification.Run(httpRequest, caseId, materialId);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);

        var operationResults = (CompleteReclassificationResponse)okResult.Value;
        Assert.True(operationResults.overallSuccess);
        Assert.Equal("Success", operationResults.status);

        Assert.Equal(expectedResponse, operationResults);

        Assert.Contains(this.mockLogger.Logs, log =>
           log.LogLevel == LogLevel.Information &&
           log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} CompleteReclassification function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
           log.LogLevel == LogLevel.Information &&
           log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [{caseId}] CompleteReclassification function completed"));
    }

    /// <summary>
    /// Tests CompleteReclassificationAsync when all expected reclassification operations succeed.
    /// </summary>
    /// <returns> Asynchronous unit test result.</returns>
    [Fact]
    public async Task Run_ReturnsMultStatusResultWithOperationsResultobject_WhenSomeOperationsSucceed()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";
        int caseId = 232;
        int materialId = 123;
        int newWitnessId = 765;
        var cmsAuthValues = new CmsAuthValues("valid cookies", "valid token", Guid.NewGuid());
        var transactionId = Guid.NewGuid();

        var reclassifyCaseMaterialRequest = CreateMockReclassifyToOtherRequest();
        var addCaseActionPlanRequest = CreateMockAddCaseActionPlanRequest();
        var addWitnessRequest = new WitnessRequest(caseId, newWitnessId, string.Empty, string.Empty);

        var completeReclassificationRequest = new CompleteReclassificationRequest(
            reclassifyCaseMaterialRequest,
            addCaseActionPlanRequest,
            addWitnessRequest);

        string httpRequestString = JsonSerializer.Serialize(completeReclassificationRequest);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var errorMessage = "Add action plan failed.";

        // Expected reponse containing all operation results.
        var expectedResponse = new CompleteReclassificationResponse(
            overallSuccess: false,
            status: "PartialSuccess",
            materialId: materialId,
            transactionId: transactionId.ToString(),
            new(true, "ReclassifyCaseMaterial", null, 12121),
            renameMaterialResult: null,
            new(false, "AddCaseActionPlan", errorMessage, null),
            new(true, "AddWitness", null, 543),
            errors: []);

        // Mock the material occhestration service to return the expected response.
        this.mockOrchestrationService
            .Setup(x => x.CompleteReclassificationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CompleteReclassificationRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        IActionResult result = await this.sutCompleteReclassification.Run(httpRequest, 321, 123);

        // Assert
        ObjectResult mutliStatusResult = Assert.IsType<ObjectResult>(result);
        Assert.NotNull(mutliStatusResult.Value);
        Assert.Equal(207, mutliStatusResult.StatusCode);

        var operationResults = (CompleteReclassificationResponse)mutliStatusResult.Value;
        Assert.False(operationResults.overallSuccess);
        Assert.Equal("PartialSuccess", operationResults.status);

        Assert.Equal(expectedResponse, operationResults);

        Assert.Contains(this.mockLogger.Logs, log =>
           log.LogLevel == LogLevel.Information &&
           log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} CompleteReclassification function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
           log.LogLevel == LogLevel.Information &&
           log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Milestone: caseId [321] CompleteReclassification function completed"));
    }

    /// <summary>
    /// Tests CompleteReclassificationAsync returns Unprocessable Entity object result when all expected reclassification operations failed..
    /// </summary>
    /// <returns> Asynchronous unit test result.</returns>
    [Fact]
    public async Task Run_ReturnsUnprocessableEntityWithOperationsObjectResult_WhenAllOperationsFailed()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";
        int caseId = 232;
        int materialId = 123;
        int newWitnessId = 765;
        var cmsAuthValues = new CmsAuthValues("valid cookies", "valid token", Guid.NewGuid());
        var transactionId = Guid.NewGuid();

        var reclassifyCaseMaterialRequest = CreateMockReclassifyToOtherRequest();
        var addCaseActionPlanRequest = CreateMockAddCaseActionPlanRequest();
        var addWitnessRequest = new WitnessRequest(caseId, newWitnessId, string.Empty, string.Empty);

        var completeReclassificationRequest = new CompleteReclassificationRequest(
            reclassifyCaseMaterialRequest,
            addCaseActionPlanRequest,
            addWitnessRequest);

        string httpRequestString = JsonSerializer.Serialize(completeReclassificationRequest);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var errorMessage = "Add action plan failed.";

        // Expected reponse containing all operation results.
        var expectedResponse = new CompleteReclassificationResponse(
            overallSuccess: false,
            status: "Failed",
            materialId: materialId,
            transactionId: transactionId.ToString(),
            new(false, "ReclassifyCaseMaterial", errorMessage, null),
            renameMaterialResult: null,
            new(false, "AddCaseActionPlan", errorMessage, null),
            new(false, "AddWitness", null, null),
            errors: []);

        // Mock the material occhestration service to return the expected response.
        this.mockOrchestrationService
            .Setup(x => x.CompleteReclassificationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CompleteReclassificationRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        IActionResult result = await this.sutCompleteReclassification.Run(httpRequest, 123, 3434);

        // Assert
        ObjectResult unprocessableEntityObjectResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
        Assert.NotNull(unprocessableEntityObjectResult.Value);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, unprocessableEntityObjectResult.StatusCode);

        var operationResults = (CompleteReclassificationResponse)unprocessableEntityObjectResult.Value;
        Assert.False(operationResults.overallSuccess);
        Assert.Equal("Failed", operationResults.status);

        Assert.Equal(expectedResponse, operationResults);

        Assert.Contains(this.mockLogger.Logs, log =>
           log.LogLevel == LogLevel.Information &&
           log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} CompleteReclassification function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
           log.LogLevel == LogLevel.Error &&
           log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} CompleteReclassification function encountered an invalid operation error"));
    }

    /// <summary>
    /// Tests CompleteReclassificationAsync returns Unprocessable when invalid operation exception is thrown.
    /// </summary>
    /// <returns> Asynchronous unit test result.</returns>
    [Fact]
    public async Task ReturnsUnprocessableEntityError_WhenInvalidOperationExceptionIsThrown()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";
        int caseId = 232;
        int materialId = 123;
        int newWitnessId = 765;
        var cmsAuthValues = new CmsAuthValues("valid cookies", "valid token", Guid.NewGuid());
        var transactionId = Guid.NewGuid();

        var reclassifyCaseMaterialRequest = CreateMockReclassifyToOtherRequest();
        var addCaseActionPlanRequest = CreateMockAddCaseActionPlanRequest();
        var addWitnessRequest = new WitnessRequest(caseId, newWitnessId, string.Empty, string.Empty);

        var completeReclassificationRequest = new CompleteReclassificationRequest(
            reclassifyCaseMaterialRequest,
            addCaseActionPlanRequest,
            addWitnessRequest);

        string httpRequestString = JsonSerializer.Serialize(completeReclassificationRequest);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var errorMessage = "Add action plan failed.";

        // Expected reponse containing all operation results.
        var expectedResponse = new CompleteReclassificationResponse(
            overallSuccess: false,
            status: "Failed",
            materialId: materialId,
            transactionId: transactionId.ToString(),
            new(false, "ReclassifyCaseMaterial", errorMessage, null),
            renameMaterialResult: null,
            new(false, "AddCaseActionPlan", errorMessage, null),
            new(false, "AddWitness", null, null),
            errors: []);

        this.mockOrchestrationService
            .Setup(x => x.CompleteReclassificationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CompleteReclassificationRequest>()))
            .ThrowsAsync(new InvalidOperationException("Invalid operation error"));

        // Act
        IActionResult result = await this.sutCompleteReclassification.Run(httpRequest, 123, 3433);

        // Assert
        UnprocessableEntityObjectResult unprocessableEntityObjectResult = Assert.IsType<UnprocessableEntityObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, unprocessableEntityObjectResult.StatusCode);

        Assert.NotNull(unprocessableEntityObjectResult.Value);

        Assert.Contains(this.mockLogger.Logs, log =>
           log.LogLevel == LogLevel.Information &&
           log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} CompleteReclassification function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
           log.LogLevel == LogLevel.Error &&
           log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} CompleteReclassification function encountered an invalid operation error: Invalid operation error"));
    }

    /// <summary>
    /// Tests that the function returns an unauthorized error when unauthorized exception is thrown.
    /// </summary>
    /// <returns> Asynchronous unit test result.</returns>
    [Fact]
    public async Task Run_ReturnsUnauthorizedError_WhenUnauthorizedAccessExceptionIsThrown()
    {
        // Arrange
        HttpRequest httpRequest = new DefaultHttpContext().Request;
        httpRequest.Headers.Cookie = "valid_cookie";
        int caseId = 232;
        int materialId = 123;
        int newWitnessId = 765;
        var cmsAuthValues = new CmsAuthValues("valid cookies", "valid token", Guid.NewGuid());
        var transactionId = Guid.NewGuid();

        var reclassifyCaseMaterialRequest = CreateMockReclassifyToOtherRequest();
        var addCaseActionPlanRequest = CreateMockAddCaseActionPlanRequest();
        var addWitnessRequest = new WitnessRequest(caseId, newWitnessId, string.Empty, string.Empty);

        var completeReclassificationRequest = new CompleteReclassificationRequest(
            reclassifyCaseMaterialRequest,
            addCaseActionPlanRequest,
            addWitnessRequest);

        string httpRequestString = JsonSerializer.Serialize(completeReclassificationRequest);

        byte[] byteArray = Encoding.UTF8.GetBytes(httpRequestString);
        var stream = new MemoryStream(byteArray);
        httpRequest.Body = stream;

        var errorMessage = "Add action plan failed.";

        // Expected reponse containing all operation results.
        var expectedResponse = new CompleteReclassificationResponse(
            overallSuccess: false,
            status: "Failed",
            materialId: materialId,
            transactionId: transactionId.ToString(),
            new(false, "ReclassifyCaseMaterial", errorMessage, null),
            renameMaterialResult: null,
            new(false, "AddCaseActionPlan", errorMessage, null),
            new(false, "AddWitness", null, null),
            errors: []);

        this.mockOrchestrationService
            .Setup(x => x.CompleteReclassificationAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CmsAuthValues>(), It.IsAny<CompleteReclassificationRequest>()))
            .ThrowsAsync(new UnauthorizedAccessException("Unauthorised"));

        // Act
        IActionResult result = await this.sutCompleteReclassification.Run(httpRequest, 123, 3434);

        // Assert
        UnauthorizedObjectResult unauthorizedAccessResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedAccessResult.StatusCode);

        Assert.NotNull(unauthorizedAccessResult.Value);

        Assert.Contains(this.mockLogger.Logs, log =>
           log.LogLevel == LogLevel.Information &&
           log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} CompleteReclassification function processed a request."));

        Assert.Contains(this.mockLogger.Logs, log =>
           log.LogLevel == LogLevel.Error &&
           log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} CompleteReclassification function encountered an unauthorized access error: Unauthorised"));
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

    private static Mock<HttpRequest> SetUpMockRequest(Stream body)
    {
        var mockRequest = new Mock<HttpRequest>();

        // Set up a DefaultHttpContext to support setting headers
        var context = new DefaultHttpContext();
        mockRequest.Setup(r => r.HttpContext).Returns(context);
        mockRequest.Setup(r => r.Headers.Add("corelation", "1232131231"));

        if (body != null)
        {
            mockRequest.Setup(r => r.Body == body);
        }

        return mockRequest;
    }
}
