// <copyright file="GetPcdReviewDetailsTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace HkuiFunctionsIntegrationTests;

using System;
using System.Threading;
using Common.Dto.Response.HouseKeeping;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Functions.HouseKeeping;
using Xunit.Abstractions;

/// <summary>
/// Integration tests for GetPcdReviewDetails function.
/// Tests the endpoint: GET /urns/{urn}/cases/{caseId}/history/{historyId}/pcd-review-details
/// </summary>
public class GetPcdReviewDetailsTests : TestBase
{
    private readonly GetPcdReviewDetails sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPcdReviewDetailsTests"/> class.
    /// </summary>
    /// <param name="testOutputHelper">The test output helper used to log test output.</param>
    public GetPcdReviewDetailsTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        var loggerMock = new Mock<ILogger<GetPcdReviewDetails>>();

        this.sut = new GetPcdReviewDetails(
            logger: loggerMock.Object,
            this.communicationService!);
    }

    /// <summary>
    /// GetPcdReviewDetails - Success case with valid URN, case ID, and history ID.
    /// Request: GET /urns/54KR7689125/cases/2179140/history/4472765/pcd-review-details
    /// Returns: PreChargeDecisionAnalysisOutcome with all properties populated.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPcdReviewDetails_WithValidUrnCaseIdHistoryId_Should_ReturnOkResultWithPreChargeDecisionAnalysis()
    {
        // Arrange
        string urn = "54KR7689125";
        int caseId = 2179140;
        int historyId = 4472765;

        (int configCaseId, int _, string username, string password) = this.GetTestConfiguration();
        AuthenticationContext authContext = await this.GetAuthenticationContextAsync(username, password);
        this.baseRequest = this.CreateHttpRequestWithCookie(caseId, authContext);

        // Act
        IActionResult response = await this.sut.Run(this.baseRequest, caseId, historyId, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(response);
        PcdReviewDetailResponse result = Assert.IsType<PcdReviewDetailResponse>(okResult.Value);

        Assert.NotNull(result);
        result.PreChargeDecisionAnalysisOutcome.Should().NotBeNull();

        // Validate PreChargeDecisionAnalysisOutcome properties
        var pcdAnalysis = result.PreChargeDecisionAnalysisOutcome!;
        pcdAnalysis.CaseId.Should().Be(caseId);
        pcdAnalysis.Id.Should().Be(historyId);
        pcdAnalysis.ReviewSummary.Should().Be("PCD Case Analysis");
        pcdAnalysis.IsCompleted.Should().BeTrue();
        pcdAnalysis.EventDate.Should().Be("27/01/2026");
        pcdAnalysis.Allocation.Should().Be("test test testing test test 2");
        pcdAnalysis.CaseSummary.Should().Be("test test testing test test 2");
        pcdAnalysis.ConsultationType.Should().Be("Full Code Test");
        pcdAnalysis.TrialStrategy.Should().Be("test test testing test test 2");
    }

    /// <summary>
    /// GetPcdReviewDetails - Verify monitoring codes collection is populated.
    /// Response should include Asset Recovery, Child Abuse, and Pre-Charge Decision monitoring codes.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPcdReviewDetails_Should_ReturnMonitoringCodes()
    {
        // Arrange
        int caseId = 2179140;
        int historyId = 4472765;

        (int configCaseId, int _, string username, string password) = this.GetTestConfiguration();
        AuthenticationContext authContext = await this.GetAuthenticationContextAsync(username, password);
        this.baseRequest = this.CreateHttpRequestWithCookie(caseId, authContext);

        // Act
        IActionResult response = await this.sut.Run(this.baseRequest, caseId, historyId, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(response);
        PcdReviewDetailResponse result = Assert.IsType<PcdReviewDetailResponse>(okResult.Value);

        var pcdAnalysis = result.PreChargeDecisionAnalysisOutcome!;
        pcdAnalysis.MonitoringCodes.Should().NotBeNull();
        pcdAnalysis.MonitoringCodes!.Count.Should().BeGreaterThan(0);

        // Verify key monitoring codes exist
        var monitoringDescriptions = pcdAnalysis.MonitoringCodes.Select(m => m.Description).ToList();
        monitoringDescriptions.Should().Contain("Asset Recovery");
        monitoringDescriptions.Should().Contain("Child Abuse");
        monitoringDescriptions.Should().Contain("Pre-Charge Decision");
    }

    /// <summary>
    /// GetPcdReviewDetails - Verify linked case URNs are returned.
    /// Response should include collection of linked case URNs with ASN indicators.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPcdReviewDetails_Should_ReturnLinkedCaseUrns()
    {
        // Arrange
        int caseId = 2179140;
        int historyId = 4472765;

        (int configCaseId, int _, string username, string password) = this.GetTestConfiguration();
        AuthenticationContext authContext = await this.GetAuthenticationContextAsync(username, password);
        this.baseRequest = this.CreateHttpRequestWithCookie(caseId, authContext);

        // Act
        IActionResult response = await this.sut.Run(this.baseRequest, caseId, historyId, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(response);
        PcdReviewDetailResponse result = Assert.IsType<PcdReviewDetailResponse>(okResult.Value);

        var pcdAnalysis = result.PreChargeDecisionAnalysisOutcome!;
        pcdAnalysis.LinkedCaseUrns.Should().NotBeNull();
        pcdAnalysis.LinkedCaseUrns!.Count.Should().BeGreaterThan(0);

        // Verify linked URN structure
        var firstLinkedUrn = pcdAnalysis.LinkedCaseUrns!.First();
        firstLinkedUrn.Urn.Should().NotBeNullOrEmpty();
        firstLinkedUrn.Asn.Should().Be("Yes");
    }

    /// <summary>
    /// GetPcdReviewDetails - Verify DG Summary and assessment details are present.
    /// Response should include Data Gathering (DG) Summary and DG Details with assessment information.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPcdReviewDetails_Should_ReturnDgSummaryAndDetails()
    {
        // Arrange
        int caseId = 2179140;
        int historyId = 4472765;

        (int configCaseId, int _, string username, string password) = this.GetTestConfiguration();
        AuthenticationContext authContext = await this.GetAuthenticationContextAsync(username, password);
        this.baseRequest = this.CreateHttpRequestWithCookie(caseId, authContext);

        // Act
        IActionResult response = await this.sut.Run(this.baseRequest, caseId, historyId, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(response);
        PcdReviewDetailResponse result = Assert.IsType<PcdReviewDetailResponse>(okResult.Value);

        var pcdAnalysis = result.PreChargeDecisionAnalysisOutcome!;
        pcdAnalysis.DgSummary.Should().Be("Yes");
        pcdAnalysis.DgDetails.Should().NotBeNull();
        pcdAnalysis.DgDetails!.AssessmentApplicable.Should().BeTrue();
        pcdAnalysis.DgDetails!.StageAssessmentCompleted.Should().Be("PCD");
        pcdAnalysis.DgDetails!.SubmissionDgCompliant.Should().Be("Yes");
        pcdAnalysis.DgDetails!.DgAssessmentItems.Should().NotBeNull();
    }

    /// <summary>
    /// GetPcdReviewDetails - Return bad request when case ID is invalid (0).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPcdReviewDetails_WithInvalidCaseId_Should_ReturnBadRequest()
    {
        // Arrange
        int invalidCaseId = 0;
        int historyId = 4472765;
        this.baseRequest = this.CreateHttpRequestWithoutCookie();

        // Act
        IActionResult response = await this.sut.Run(this.baseRequest, invalidCaseId, historyId, CancellationToken.None);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        Assert.NotNull(badRequestResult);
        badRequestResult!.Value.Should().BeOfType<string>();
        (badRequestResult!.Value as string).Should().Contain("Invalid case Id");
    }

    private HttpRequest CreateHttpRequestWithoutCookie()
    {
        var context = new DefaultHttpContext();
        return context.Request;
    }
}
