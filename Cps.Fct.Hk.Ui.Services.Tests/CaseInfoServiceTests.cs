// <copyright file="CaseInfoServiceTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services.Tests;

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Interfaces.Model;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using DdeiClient.Clients.Interfaces;
using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.HouseKeeping;

/// <summary>
/// Contains unit tests for the <see cref="CaseInfoService"/> class.
/// </summary>
public class CaseInfoServiceTests
{
    private readonly TestLogger<CaseInfoService> mockLogger;
    private readonly Mock<IMasterDataServiceClient> apiClientMock;
    private readonly CaseInfoService caseInfoService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CaseInfoServiceTests"/> class.
    /// Sets up the test dependencies, including a mock logger and API client.
    /// </summary>
    public CaseInfoServiceTests()
    {
        this.mockLogger = new TestLogger<CaseInfoService>();
        this.apiClientMock = new Mock<IMasterDataServiceClient>();

        // Initialize the service with mocked dependencies
        this.caseInfoService = new CaseInfoService(this.mockLogger, this.apiClientMock.Object);
    }

    /// <summary>
    /// Tests that GetCaseInfoAsync successfully retrieves case information when provided valid inputs.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetCaseInfoAsync_ReturnsCaseSummary_WhenApiCallIsSuccessful()
    {
        // Arrange
        int caseId = 123;
        var cmsAuthValues = new CmsAuthValues("validCmsToken", "validCmsCookies");

        // Setup the expected CaseSummary result
        var expectedCaseSummary = new CaseSummaryResponse(caseId, "06SC1234572", "Will", "SMITH", 2, "Hull UT");

        // Setup the API client to return null for the case summary
        this.apiClientMock
            .Setup(x => x.GetCaseSummaryAsync(It.IsAny<GetCaseSummaryRequest>(), cmsAuthValues))
            .ReturnsAsync(expectedCaseSummary);

        // Act
        var result = await this.caseInfoService.GetCaseInfoAsync(caseId, cmsAuthValues);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<CaseSummaryResponse>(result);
        Assert.Equal(expectedCaseSummary.CaseId, result.CaseId);
        Assert.Equal(expectedCaseSummary.LeadDefendantFirstNames, result.LeadDefendantFirstNames);
        Assert.Equal(expectedCaseSummary.LeadDefendantSurname, result.LeadDefendantSurname);
        Assert.Equal(expectedCaseSummary.NumberOfDefendants, result.NumberOfDefendants);
        Assert.Equal(expectedCaseSummary.UnitName, result.UnitName);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Fetching info for caseId [{caseId}]"));
    }

    /// <summary>
    /// Tests that GetCaseInfoAsync throws an InvalidOperationException when no case summary is found.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetCaseInfoAsync_ThrowsInvalidOperationException_WhenCaseSummaryNotFound()
    {
        // Arrange
        int caseId = 123;
        var cmsAuthValues = new CmsAuthValues("validCmsToken", "validCmsCookies");

        // Setup the API client to return null for the case summary
        this.apiClientMock
            .Setup(x => x.GetCaseSummaryAsync(It.IsAny<GetCaseSummaryRequest>(), cmsAuthValues))
            .ReturnsAsync((CaseSummaryResponse?)null);

        // Act & Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            this.caseInfoService.GetCaseInfoAsync(caseId, cmsAuthValues));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Fetching info for caseId [{caseId}]"));

        Assert.Equal($"{LoggingConstants.HskUiLogPrefix} No case summary found for caseId [{caseId}]", exception.Message);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching case information for caseId [{caseId}]"));
    }

    /// <summary>
    /// Tests that GetCaseInfoAsync logs an error and throws an exception when the API call fails.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetCaseInfoAsync_ThrowsException_WhenApiCallFails()
    {
        // Arrange
        int caseId = 123;
        var cmsAuthValues = new CmsAuthValues("validCmsToken", "validCmsCookies");

        // Setup the API client to throw an exception
        this.apiClientMock
            .Setup(x => x.GetCaseSummaryAsync(It.IsAny<GetCaseSummaryRequest>(), cmsAuthValues))
            .ThrowsAsync(new Exception("DDEI-EAS API error"));

        // Act & Assert
        Exception exception = await Assert.ThrowsAsync<Exception>(() =>
            this.caseInfoService.GetCaseInfoAsync(caseId, cmsAuthValues));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Fetching info for caseId [{caseId}]"));

        Assert.Equal("DDEI-EAS API error", exception.Message);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching case information for caseId [{caseId}]"));
    }
}
