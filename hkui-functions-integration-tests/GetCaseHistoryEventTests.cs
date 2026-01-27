// <copyright file="GetCaseHistoryEventTests.cs" company="TheCrownProsecutionService">
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
/// Integration tests for GetCaseHistoryEvent function.
/// </summary>
public class GetCaseHistoryEventTests : TestBase
{
    private readonly GetCaseHistoryEvent sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCaseHistoryEventTests"/> class.
    /// </summary>
    /// <param name="testOutputHelper">The test output helper used to log test output.</param>
    public GetCaseHistoryEventTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        var loggerMock = new Mock<ILogger<GetCaseHistoryEvent>>();

        this.sut = new GetCaseHistoryEvent(
            logger: loggerMock.Object,
            this.communicationService!);
    }

    /// <summary>
    /// GetCaseHistoryEvent - success run as expected,.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetCaseHistoryEvent_Should_ReturnAsExpected()
    {
        // Arrange
        this.baseCaseId = 2158751;
        this.ValidateTestDependencies();

        (int caseId, int _, string username, string password) = this.GetTestConfiguration();
        AuthenticationContext authContext = await this.GetAuthenticationContextAsync(username, password);
        this.baseRequest = this.CreateHttpRequestWithCookie(this.baseCaseId, authContext);

        // Act
        IActionResult response = await this.sut.Run(this.baseRequest, this.baseCaseId);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(response);
        List<HistoryEvent> result = Assert.IsType<List<HistoryEvent>>(okResult.Value);

        // Assert
        result.Count.Should().BeGreaterThanOrEqualTo(17);
        var preChargeDecision = result.Where(x => x.Type == HistoryEventType.PreChargeDecision).OrderBy(x => x.Id);
        preChargeDecision.Count().Should().BeGreaterThanOrEqualTo(2);
        preChargeDecision.First().Name.Should().Be("Pre-charge Decision");
        preChargeDecision.First().Id.Should().Be(4365090);
        preChargeDecision.First().AuthorOrVenue.Should().Be("Tim Bates");
    }
}
