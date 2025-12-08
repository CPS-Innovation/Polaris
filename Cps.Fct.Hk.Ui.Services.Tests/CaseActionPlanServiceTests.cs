// <copyright file="CaseActionPlanServiceTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services.Tests;

using System.Threading.Tasks;
using Common.Constants;
using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.HouseKeeping;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.Interfaces.Exceptions;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using DdeiClient.Clients.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Contains unit tests for the <see cref="CaseActionPlanService"/> class.
/// </summary>
public class CaseActionPlanServiceTests
{
    private readonly TestLogger<CaseActionPlanService> mockLogger;
    private readonly Mock<ICaseLockService> caseLockService;
    private readonly Mock<IMasterDataServiceClient> apiClientMock;
    private readonly CaseActionPlanService caseActionPlanService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CaseActionPlanServiceTests"/> class.
    /// Sets up the test dependencies, including a mock logger and API client.
    /// </summary>
    public CaseActionPlanServiceTests()
    {
        this.mockLogger = new TestLogger<CaseActionPlanService>();
        this.caseLockService = new Mock<ICaseLockService>();
        this.apiClientMock = new Mock<IMasterDataServiceClient>();

        // Initialize the service with mocked dependencies
        this.caseActionPlanService = new CaseActionPlanService(
            this.mockLogger,
            this.caseLockService.Object,
            this.apiClientMock.Object);
    }

    /// <summary>
    /// Tests that AddCaseActionPlanAsync successfully adds a case action plan when provided valid inputs.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task AddCaseActionPlanAsync_ReturnsNoContentResult_WhenApiCallIsSuccessful()
    {
        // Arrange
        string urn = "1234567890";
        int caseId = 123;
        var cmsAuthValues = new CmsAuthValues("validCmsToken", "validCmsCookies");

        AddCaseActionPlanRequest mockAddCaseActionPlanRequest = CreateMockAddCaseActionPlanRequest();

        // Setup the API client to return 204 (NoContentResult)
        this.apiClientMock
            .Setup(x => x.AddCaseActionPlanAsync(It.IsAny<int>(), It.IsAny<AddActionPlanRequest>(), cmsAuthValues))
            .ReturnsAsync(new NoContentResult());

        this.caseLockService
        .Setup(x => x.CheckCaseLockAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
        .ReturnsAsync(new CaseLockedStatusResult
        {
            IsLocked = false,
        });

        // Act
        NoContentResult result = await this.caseActionPlanService.AddCaseActionPlanAsync(
            urn,
            caseId,
            mockAddCaseActionPlanRequest,
            cmsAuthValues);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<NoContentResult>(result);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Attempting to add case action plan for caseId [{caseId}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Successfully added case action plan to caseId [{caseId}]"));
    }

    /// <summary>
    /// Tests that AddCaseActionPlanAsync logs an error and throws an exception when the API call fails.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task AddCaseActionPlanAsync_ThrowsException_WhenApiCallFails()
    {
        // Arrange
        string urn = "1234567890";
        int caseId = 123;
        var cmsAuthValues = new CmsAuthValues("validCmsToken", "validCmsCookies");

        AddCaseActionPlanRequest mockAddCaseActionPlanRequest = CreateMockAddCaseActionPlanRequest();

        // Setup the API client to throw an exception
        this.apiClientMock
            .Setup(x => x.AddCaseActionPlanAsync(It.IsAny<int>(), It.IsAny<AddActionPlanRequest>(), cmsAuthValues))
            .ThrowsAsync(new Exception("DDEI-EAS API error"));

        this.caseLockService
          .Setup(x => x.CheckCaseLockAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
          .ReturnsAsync(new CaseLockedStatusResult
          {
              IsLocked = false,
          });

        // Act & Assert
        Exception exception = await Assert.ThrowsAsync<Exception>(() =>
            this.caseActionPlanService.AddCaseActionPlanAsync(
                urn,
                caseId,
                mockAddCaseActionPlanRequest,
                cmsAuthValues));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Attempting to add case action plan for caseId [{caseId}]"));

        Assert.Equal("DDEI-EAS API error", exception.Message);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Failed to add case action plan to caseId [{caseId}]"));
    }

    /// <summary>
    /// Tests that AddCaseActionPlanAsync logs an error when the API call to release the case lock fails.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task AddCaseActionPlanAsync_LogsError_WhenCaseLocked()
    {
        // Arrange
        string urn = "1234567890";
        int caseId = 123;
        var cmsAuthValues = new CmsAuthValues("validCmsToken", "validCmsCookies");

        AddCaseActionPlanRequest mockAddCaseActionPlanRequest = CreateMockAddCaseActionPlanRequest();

        // Simulate successful API call
        this.apiClientMock
            .Setup(x => x.AddCaseActionPlanAsync(It.IsAny<int>(), It.IsAny<AddActionPlanRequest>(), cmsAuthValues))
            .ReturnsAsync(new NoContentResult());

        this.caseLockService
        .Setup(x => x.CheckCaseLockAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
        .ReturnsAsync(new CaseLockedStatusResult
        {
            IsLocked = true,
        });

        // Act
        Exception exception = await Assert.ThrowsAsync<CaseLockedException>(() =>
        this.caseActionPlanService.AddCaseActionPlanAsync(
            urn,
            caseId,
            mockAddCaseActionPlanRequest,
            cmsAuthValues));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<CaseLockedException>(exception);
    }

    /// <summary>
    /// Creates a mock AddCaseActionPlanRequest object for testing purposes.
    /// </summary>
    /// <param name="modify">
    /// An optional action to modify the generated AddCaseActionPlanRequest object before returning it.
    /// </param>
    /// <returns>A mock AddCaseActionPlanRequest object with pre-filled test data.</returns>
    private static AddCaseActionPlanRequest CreateMockAddCaseActionPlanRequest(Action<AddCaseActionPlanRequest>? modify = null)
    {
        var request = new AddCaseActionPlanRequest(
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

        modify?.Invoke(request);

        return request;
    }
}
