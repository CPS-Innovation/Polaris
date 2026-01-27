// <copyright file="GetPreChargeDecisionByHistoryIdTests.cs" company="TheCrownProsecutionService">
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
/// Integration tests for GetPreChargeDecisionByHistoryId function.
/// </summary>
public class GetPreChargeDecisionByHistoryIdTests : TestBase
{
    private readonly GetPreChargeDecisionByHistoryId sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPreChargeDecisionByHistoryIdTests"/> class.
    /// </summary>
    /// <param name="testOutputHelper">The test output helper used to log test output.</param>
    public GetPreChargeDecisionByHistoryIdTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        var loggerMock = new Mock<ILogger<GetPreChargeDecisionByHistoryId>>();

        this.sut = new GetPreChargeDecisionByHistoryId(
            logger: loggerMock.Object,
            this.communicationService);
    }

    /// <summary>
    /// GetPreChargeDecisionByHistoryId - success run as expected,.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPreChargeDecisionByHistoryId_Should_ReturnAsExpected()
    {
        // Arrange
        this.baseCaseId = 2158751;
        (int caseId, int _, string username, string password) = this.GetTestConfiguration();
        AuthenticationContext authContext = await this.GetAuthenticationContextAsync(username, password);
        this.baseRequest = this.CreateHttpRequestWithCookie(this.baseCaseId, authContext);

        // Act
        IActionResult response = await this.sut.Run(this.baseRequest, this.baseCaseId, 4365090);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(response);
        PreChargeDecisionOutcome result = Assert.IsType<PreChargeDecisionOutcome>(okResult.Value);

        // Assert
        Assert.Equal(result.CaseId, this.baseCaseId);
        result.CaseId.Should().Be(2158751);
        result.Id.Should().Be(4365090);
        result.IsCompleted.Should().Be(true);
        result.EventDate.Should().Be("26/02/2025");
        result.Author.Should().Be("Tim Bates");
        result.DecisionRequestedDate.Should().Be("20/02/2025");
        result.Method.Should().Be("Area");
        result.InvestigationStage.Should().Be("Post Interview");
        result.DecisionMadeBy.Should().Be("Tim Bates");
        result.ActionPlan.Should().Be(true);
        result.PoliceCovidUrgency.Should().Be("Other");
        result.PcdHistoryActionPlan.Should().HaveCountGreaterThan(0);
        result.PcdHistoryActionPlan.FirstOrDefault().Should().NotBeNull();
        result.PcdHistoryActionPlan.First().ActionType.Should().Be("Action Plan");
        result.PcdHistoryActionPlan.First().EntryDate.Should().Be("26/02/2025");
        result.PcdHistoryActionPlan.First().Status.Should().Be("Sent on PCD Response");
        result.PcdHistoryActionPlan.First().Suspect.Should().Be("All");
        result.PcdHistoryActionPlan.First().ActionPoint.Should().Be("Requested:\n- Key Witness Details;\n(Test action plan during EA to make sure a dummy is not raised.)");

        result.Urn.Should().Be("16XL5110225");
        result.DefendantDecisions.Count().Should().Be(2);
    }

    /// <summary>
    /// GetPreChargeDecisionByHistoryId - Return not found event result.,.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPreChargeDecisionByHistoryId_WheNotFound_Should_Return404()
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
