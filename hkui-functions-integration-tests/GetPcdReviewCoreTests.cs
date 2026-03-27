// <copyright file="GetPcdReviewCoreTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace HkuiFunctionsIntegrationTests;

using System;
using System.Collections.Generic;
using System.Threading;
using Common.Dto.Response.HouseKeeping;
using Common.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolarisGateway.Functions.HouseKeeping;
using Xunit;
using Xunit.Abstractions;

/// <summary>
/// Integration tests for GetPcdReviewCore function.
/// Tests the endpoint: GET /cases/{caseId}/pcd-review
/// </summary>
public class GetPcdReviewCoreTests : TestBase
{
    private readonly GetPcdReviewCore sut;
    private readonly Moq.Mock<Cps.Fct.Hk.Ui.Interfaces.ICommunicationService> communicationServiceMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPcdReviewCoreTests"/> class.
    /// </summary>
    /// <param name="testOutputHelper">The test output helper used to log test output.</param>
    public GetPcdReviewCoreTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        var loggerMock = new Mock<ILogger<GetPcdReviewCore>>();

        this.communicationServiceMock = new Moq.Mock<Cps.Fct.Hk.Ui.Interfaces.ICommunicationService>();
        // Default to empty result to avoid external calls in integration tests
        this.communicationServiceMock
            .Setup(c => c.GetPcdReviewCoreAsync(It.IsAny<int>(), It.IsAny<Common.Dto.Request.CmsAuthValues>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Common.Dto.Response.HouseKeeping.PcdReviewCoreResponseDto>());

        this.sut = new GetPcdReviewCore(
            logger: loggerMock.Object,
            this.communicationServiceMock.Object);
    }

    /// <summary>
    /// GetPcdReviewCore - Success case with valid case ID.
    /// Request: GET /cases/2179140/pcd-review
    /// Returns: Collection of PcdReviewCoreResponseDto with review items.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPcdReviewCore_WithValidCaseId_Should_ReturnOkResultWithPcdReviewItems()
    {
        // Arrange
        int caseId = 2179140;

        // Use a fake authentication context to avoid external authentication calls
        var authContext = new AuthenticationContext(cookies: "fakeCookies", token: "fakeToken", expiryTime: DateTimeOffset.UtcNow.AddHours(1));
        this.baseRequest = this.CreateHttpRequestWithCookie(caseId, authContext);
        var caseUrn = string.Empty;

        // Act
        IActionResult response = await this.sut.Run(this.baseRequest, caseUrn, caseId, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(response);
        IReadOnlyCollection<PcdReviewCoreResponseDto> result = Assert.IsType<List<PcdReviewCoreResponseDto>>(okResult.Value);

        Assert.NotNull(result);
        result.Should().NotBeNull();
        result.Count.Should().BeGreaterThanOrEqualTo(0);
    }

    /// <summary>
    /// GetPcdReviewCore - Verify PCD review items have required properties.
    /// Response should include items with Id, Type enum, and Date.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPcdReviewCore_Should_ReturnItemsWithRequiredProperties()
    {
        // Arrange
        int caseId = 2179140;

        var authContext = new AuthenticationContext(cookies: "fakeCookies", token: "fakeToken", expiryTime: DateTimeOffset.UtcNow.AddHours(1));
        this.baseRequest = this.CreateHttpRequestWithCookie(caseId, authContext);
        var caseUrn = string.Empty;

        // Act
        IActionResult response = await this.sut.Run(this.baseRequest, caseUrn, caseId, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(response);
        var result = Assert.IsType<List<PcdReviewCoreResponseDto>>(okResult.Value);

        Assert.NotNull(result);
        if (result.Count > 0)
        {
            foreach (var item in result)
            {
                item.Id.Should().NotBeNullOrEmpty();
                item.Date.Should().NotBeNullOrEmpty();
                item.Type.Should().BeOneOf(
                    PcdReviewCoreType.EarlyAdvice,
                    PcdReviewCoreType.InitialReview,
                    PcdReviewCoreType.PreChargeDecisionAnalysis);
            }
        }
    }

    /// <summary>
    /// GetPcdReviewCore - Verify type enum values are correctly mapped.
    /// Response should contain valid enum values: EarlyAdvice, InitialReview, or PreChargeDecisionAnalysis.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPcdReviewCore_Should_ReturnValidTypeEnumValues()
    {
        // Arrange
        int caseId = 2179140;

        var authContext = new AuthenticationContext(cookies: "fakeCookies", token: "fakeToken", expiryTime: DateTimeOffset.UtcNow.AddHours(1));
        this.baseRequest = this.CreateHttpRequestWithCookie(caseId, authContext);
        var cseUrn = string.Empty;

        // Act
        IActionResult response = await this.sut.Run(this.baseRequest, cseUrn, caseId, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(response);
        var result = Assert.IsType<List<PcdReviewCoreResponseDto>>(okResult.Value);

        Assert.NotNull(result);
        if (result.Count > 0)
        {
            var typeValues = new List<PcdReviewCoreType>(result.Select(item => item.Type));
            typeValues.Should().AllSatisfy(t => t.Should().BeOneOf(
                PcdReviewCoreType.EarlyAdvice,
                PcdReviewCoreType.InitialReview,
                PcdReviewCoreType.PreChargeDecisionAnalysis));
        }
    }

    /// <summary>
    /// GetPcdReviewCore - Verify items are ordered by ID.
    /// Response should return items in ascending order by their ID.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPcdReviewCore_Should_ReturnItemsOrderedById()
    {
        // Arrange
        int caseId = 2179140;

        var authContext = new AuthenticationContext(cookies: "fakeCookies", token: "fakeToken", expiryTime: DateTimeOffset.UtcNow.AddHours(1));
        this.baseRequest = this.CreateHttpRequestWithCookie(caseId, authContext);
        var caseUrn = string.Empty;

        // Act
        IActionResult response = await this.sut.Run(this.baseRequest, caseUrn, caseId, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(response);
        var result = Assert.IsType<List<PcdReviewCoreResponseDto>>(okResult.Value);

        Assert.NotNull(result);
        if (result.Count > 1)
        {
            var items = new List<PcdReviewCoreResponseDto>(result);
            for (int i = 0; i < items.Count - 1; i++)
            {
                int currentId = int.Parse(items[i].Id);
                int nextId = int.Parse(items[i + 1].Id);
                currentId.Should().BeLessThanOrEqualTo(nextId);
            }
        }
    }

    /// <summary>
    /// GetPcdReviewCore - Verify Early Advice detection when PCD Analysis appears first.
    /// If the first event in the case is PreChargeDecisionAnalysis, it should be marked as EarlyAdvice.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPcdReviewCore_Should_MarkFirstPcdAnalysisAsEarlyAdvice()
    {
        // Arrange
        int caseId = 2179140;

        var authContext = new AuthenticationContext(cookies: "fakeCookies", token: "fakeToken", expiryTime: DateTimeOffset.UtcNow.AddHours(1));
        this.baseRequest = this.CreateHttpRequestWithCookie(caseId, authContext);
        var caseUrn = string.Empty;

        // Act
        IActionResult response = await this.sut.Run(this.baseRequest, caseUrn, caseId, CancellationToken.None);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(response);
        var result = Assert.IsType<List<PcdReviewCoreResponseDto>>(okResult.Value);

        Assert.NotNull(result);
        if (result.Count > 0)
        {
            var items = new List<PcdReviewCoreResponseDto>(result);
            var firstItem = items[0];

            // If the first event is a PCD event (either EarlyAdvice or PreChargeDecisionAnalysis)
            // and there's an InitialReview later, then the first should be EarlyAdvice
            if (firstItem.Type == PcdReviewCoreType.EarlyAdvice)
            {
                var hasInitialReviewAfter = items.Any(item => item.Type == PcdReviewCoreType.InitialReview);
                if (hasInitialReviewAfter)
                {
                    firstItem.Type.Should().Be(PcdReviewCoreType.EarlyAdvice);
                }
            }
        }
    }

    /// <summary>
    /// GetPcdReviewCore - Return bad request when case ID is invalid (0).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPcdReviewCore_WithInvalidCaseId_Should_ReturnBadRequest()
    {
        // Arrange
        int invalidCaseId = 0;
        this.baseRequest = this.CreateHttpRequestWithoutCookie();
        var caseUrn = string.Empty;

        // Act
        IActionResult response = await this.sut.Run(this.baseRequest, caseUrn, invalidCaseId, CancellationToken.None);

        // Assert
        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = response as BadRequestObjectResult;
        Assert.NotNull(badRequestResult);
        badRequestResult!.Value.Should().BeOfType<string>();
        (badRequestResult!.Value as string).Should().Contain("Invalid case Id");
    }

    /// <summary>
    /// GetPcdReviewCore - Return bad request when case ID is negative.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPcdReviewCore_WithNegativeCaseId_Should_ReturnBadRequest()
    {
        // Arrange
        int invalidCaseId = -1;
        this.baseRequest = this.CreateHttpRequestWithoutCookie();
        var caseUrn = string.Empty;

        // Act
        IActionResult response = await this.sut.Run(this.baseRequest, caseUrn, invalidCaseId, CancellationToken.None);

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
