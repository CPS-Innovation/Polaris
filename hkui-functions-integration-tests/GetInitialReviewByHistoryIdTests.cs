// <copyright file="GetInitialReviewByHistoryIdTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace HkuiFunctionsIntegrationTests;

using System.Globalization;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.MasterDataService.Infrastructure.ApiClient;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Functions.HouseKeeping;
using Xunit.Abstractions;

/// <summary>
/// Integration tests for GetInitialReviewByHistoryId function.
/// </summary>
public class GetInitialReviewByHistoryIdTests : TestBase
{
    private readonly GetInitialReviewByHistoryId sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetInitialReviewByHistoryIdTests"/> class.
    /// </summary>
    /// <param name="testOutputHelper">The test output helper used to log test output.</param>
    public GetInitialReviewByHistoryIdTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        var loggerMock = new Mock<ILogger<GetInitialReviewByHistoryId>>();

        this.sut = new GetInitialReviewByHistoryId(
            logger: loggerMock.Object,
            this.communicationService!);
    }

    /// <summary>
    /// GetInitialReviewByHistoryId - success run as expected,.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetInitialReviewByHistoryId_Should_ReturnAsExpected()
    {
        // Arrange
        this.baseCaseId = 2158751;
        (int caseId, int _, string username, string password) = this.GetTestConfiguration();
        AuthenticationContext authContext = await this.GetAuthenticationContextAsync(username, password);
        this.baseRequest = this.CreateHttpRequestWithCookie(this.baseCaseId, authContext);

        // Act
        IActionResult response = await this.sut.Run(this.baseRequest, this.baseCaseId, 4365088);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(response);
        PreChargeDecisionAnalysisOutcome result = Assert.IsType<PreChargeDecisionAnalysisOutcome>(okResult.Value);

        // Assert
        Assert.Equal(result.CaseId, this.baseCaseId);
        result.CaseId.Should().Be(2158751);
        result.Allocation.Should().Be("dasd");
        result.CaseSummary.Should().Be("What advice is sought?");
        result.DisclosureActionsAndIssues.Should().Be("Materials and information considered");
        result.EuropeanCourtOfHumanRights.Should().Be(null);
        result.EvidentialAssessment.Should().Be("asd");
        result.Id.Should().Be(4365088);
        result.InstructionsToOperationsDeliveryOrAdvocate.Should().Be("ads");
        result.PublicInterestAssessment.Should().Be("asd");
        result.ConsultationType.Should().Be("Threshold Test");
        result.TrialStrategy.Should().Be("Your advice");
        result.WitnessOrVictimInformationAndActions.Should().Be("asd");
        result.ReviewSummary.Should().Be("PCD Case Analysis");
        result.ProsecutorDeclaration.Should().Be(null);
        result.IsCompleted.Should().Be(true);
        result.EventDate.Should().Be("26/02/2025");
    }

    /// <summary>
    /// GetInitialReviewByHistoryId - Return not found event result.,.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetInitialReviewByHistoryId_WheNotFound_Should_Return404()
    {
        // Arrange
        this.baseCaseId = 2158751;
        (int caseId, int _, string username, string password) = this.GetTestConfiguration();
        AuthenticationContext authContext = await this.GetAuthenticationContextAsync(username, password);
        this.baseRequest = this.CreateHttpRequestWithCookie(this.baseCaseId, authContext);

        // Act
        IActionResult response = await this.sut.Run(this.baseRequest, this.baseCaseId, 4365081);

        // Assert
        var result = Assert.IsType<StatusCodeResult>(response);
        result.StatusCode.Should().BeGreaterThanOrEqualTo(404);
    }
}
