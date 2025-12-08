// <copyright file="CaseDefendantsServiceTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace PolarisGateway.Tests.Functions.HouseKeeping;

using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using Cps.Fct.Hk.Ui.Services;
using DdeiClient.Clients.Interfaces;
using Common.Dto.Request;
using Common.Dto.Response.HouseKeeping;
using System.Collections.Generic;
using System;
using Common.Constants;
using Common.Dto.Request.HouseKeeping;

/// <summary>
/// Contains unit tests for the <see cref="CaseDefendantsService"/> class.
/// </summary>
public class CaseDefendantsServiceTests
{
    private readonly TestLogger<CaseDefendantsService> mockLogger;
    private readonly Mock<IMasterDataServiceClient> apiClientMock;
    private readonly CaseDefendantsService caseDefendantsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CaseDefendantsServiceTests"/> class.
    /// Sets up the test dependencies, including a mock logger and API client.
    /// </summary>
    public CaseDefendantsServiceTests()
    {
        this.mockLogger = new TestLogger<CaseDefendantsService>();
        this.apiClientMock = new Mock<IMasterDataServiceClient>();

        // Initialize the service with mocked dependencies
        this.caseDefendantsService = new CaseDefendantsService(this.mockLogger, this.apiClientMock.Object);
    }

    /// <summary>
    /// Tests that GetCaseDefendantsAsync successfully retrieves case defendants when provided valid inputs.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetCaseDefendantsAsync_ReturnsCaseDefendants_WhenApiCallIsSuccessful()
    {
        // Arrange
        int caseId = 123;
        var cmsAuthValues = new CmsAuthValues("validCmsToken", "validCmsCookies");

        // Setup the expected DefendantsResponse result
        var expectedCaseDefendants = new DefendantsResponse
        {
            Defendants = new List<Defendant>
            {
                new Defendant(
                    Id: 2781991,
                    CaseId: 123,
                    ListOrder: 1,
                    Type: "Person",
                    FirstNames: "Will",
                    Surname: "SMITH",
                    Dob: null,
                    PoliceRemandStatus: null,
                    Youth: false,
                    CustodyTimeLimit: null,
                    Offences: new List<Offence>
                    {
                        new Offence(
                            Id: 2257649,
                            ListOrder: null,
                            Code: "NYC",
                            Type: "CHARGE",
                            Active: "DISPOSED",
                            Description: "Not Yet Charged",
                            FromDate: null,
                            ToDate: null,
                            LatestPlea: "NO_PLEA",
                            LatestVerdict: "NO_VERDICT",
                            DisposedReason: "FINALISED",
                            LastHearingOutcome: null,
                            CustodyTimeLimit: null,
                            LatestPleaDescription: "-"),
                    },
                    Charges: new List<Charge>(),
                    ProposedCharges: new List<ProposedCharge>(),
                    NextHearing: null,
                    DefendantPcdReview: null,
                    Solicitor: null,
                    PersonalDetail: new PersonalDetail(
                        Address: null,
                        Email: null,
                        Ethnicity: "NotProvided",
                        Gender: "Male",
                        Occupation: null,
                        HomePhoneNumber: null,
                        MobilePhoneNumber: null,
                        WorkPhoneNumber: null,
                        PreferredCorrespondenceLanguage: "English",
                        Religion: "NotProvided",
                        Guardian: null)),

                new Defendant(
                    Id: 2781992,
                    CaseId: 123,
                    ListOrder: 2,
                    Type: "Person",
                    FirstNames: "Jane",
                    Surname: "DOE",
                    Dob: new DateTime(1990, 1, 1),
                    PoliceRemandStatus: null,
                    Youth: false,
                    CustodyTimeLimit: null,
                    Offences: null,
                    Charges: new List<Charge>(),
                    ProposedCharges: new List<ProposedCharge>(),
                    NextHearing: null,
                    DefendantPcdReview: null,
                    Solicitor: null,
                    PersonalDetail: new PersonalDetail(
                        Address: null,
                        Email: "jane.doe@example.com",
                        Ethnicity: "Unknown",
                        Gender: "Female",
                        Occupation: null,
                        HomePhoneNumber: null,
                        MobilePhoneNumber: null,
                        WorkPhoneNumber: null,
                        PreferredCorrespondenceLanguage: "English",
                        Religion: null,
                        Guardian: null)),
            },
        };

        // Setup the API client to return null for the case defendants
        this.apiClientMock
            .Setup(x => x.GetCaseDefendantsAsync(It.IsAny<ListCaseDefendantsRequest>(), cmsAuthValues))
            .ReturnsAsync(expectedCaseDefendants);

        // Act
        DefendantsResponse caseDefendants = await this.caseDefendantsService.GetCaseDefendantsAsync(caseId, cmsAuthValues);

        // Assert
        Assert.NotNull(caseDefendants);
        Assert.IsType<DefendantsResponse>(caseDefendants);

        Assert.NotNull(caseDefendants.Defendants);
        Assert.Equal(2, caseDefendants.Defendants.Count);

        Defendant first = caseDefendants.Defendants[0];
        Assert.Equal(2781991, first.Id);
        Assert.Equal("Will", first.FirstNames);
        Assert.Equal("SMITH", first.Surname);
        Assert.Single(first.Offences!);
        Assert.NotNull(first.Offences);
        Assert.Equal("NYC", first.Offences[0].Code);
        Assert.Equal("DISPOSED", first.Offences[0].Active);
        Assert.Null(first.Offences[0].ListOrder);

        Defendant second = caseDefendants.Defendants[1];
        Assert.Equal(2781992, second.Id);
        Assert.Equal("Jane", second.FirstNames);
        Assert.Equal("DOE", second.Surname);
        Assert.Equal(new DateTime(1990, 1, 1), second.Dob);
        Assert.Equal("Female", second.PersonalDetail?.Gender);
        Assert.Equal("jane.doe@example.com", second.PersonalDetail?.Email);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Fetching defendants for caseId [{caseId}]"));
    }

    /// <summary>
    /// Tests that GetCaseDefendantsAsync throws an InvalidOperationException when no case defendants are found.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetCaseDefendantsAsync_ThrowsInvalidOperationException_WhenCaseDefendantsNotFound()
    {
        // Arrange
        int caseId = 123;
        var cmsAuthValues = new CmsAuthValues("validCmsToken", "validCmsCookies");

        // Setup the API client to return null for the case defendants
        this.apiClientMock
            .Setup(x => x.GetCaseDefendantsAsync(It.IsAny<ListCaseDefendantsRequest>(), cmsAuthValues))
            .ReturnsAsync((DefendantsResponse?)null);

        // Act & Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            this.caseDefendantsService.GetCaseDefendantsAsync(caseId, cmsAuthValues));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Fetching defendants for caseId [{caseId}]"));

        Assert.Equal($"{LoggingConstants.HskUiLogPrefix} No case defendants found for caseId [{caseId}]", exception.Message);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching case defendants for caseId [{caseId}]"));
    }

    /// <summary>
    /// Tests that GetCaseDefendantsAsync logs an error and throws an exception when the API call fails.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetCaseDefendantsAsync_ThrowsException_WhenApiCallFails()
    {
        // Arrange
        int caseId = 123;
        var cmsAuthValues = new CmsAuthValues("validCmsToken", "validCmsCookies");

        // Setup the API client to throw an exception
        this.apiClientMock
            .Setup(x => x.GetCaseDefendantsAsync(It.IsAny<ListCaseDefendantsRequest>(), cmsAuthValues))
            .ThrowsAsync(new Exception("DDEI-EAS API error"));

        // Act & Assert
        Exception exception = await Assert.ThrowsAsync<Exception>(() =>
            this.caseDefendantsService.GetCaseDefendantsAsync(caseId, cmsAuthValues));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Fetching defendants for caseId [{caseId}]"));

        Assert.Equal("DDEI-EAS API error", exception.Message);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching case defendants for caseId [{caseId}]"));
    }
}
