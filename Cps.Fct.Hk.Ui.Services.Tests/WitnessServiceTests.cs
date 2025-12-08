// <copyright file="WitnessServiceTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Constants;
using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.HouseKeeping;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using DdeiClient.Clients.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for witness service.
/// </summary>
public class WitnessServiceTests
{
    private readonly TestLogger<WitnessService> mockLogger;
    private readonly Mock<IMasterDataServiceClient> apiClientMock;
    private readonly WitnessService sutWitnessService;
    private readonly Mock<ICaseLockService> caseLockServiceMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="WitnessServiceTests"/> class.
    /// Sets up test dependencies.
    /// </summary>
    public WitnessServiceTests()
    {
        this.mockLogger = new TestLogger<WitnessService>();
        this.apiClientMock = new Mock<IMasterDataServiceClient>();
        this.caseLockServiceMock = new Mock<ICaseLockService>();

        this.sutWitnessService = new WitnessService(this.mockLogger, this.apiClientMock.Object, this.caseLockServiceMock.Object);
    }

    /// <summary>
    /// Test getWitnessesForCase method successfully gets witnesses.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task GetWitnessesForCaseAsync_ReturnsWitnessesForCase_WhenApiCallIsSuccessful()
    {
        // Arrange
        int caseId = 4321;
        var cmsAuthValues = new CmsAuthValues("validCmsToken", "validCmsCookies");

        var expectedWitnesses = new WitnessesResponse
        {
            Witnesses = new List<Witness>()
            {
                new (caseId, 343, "Joe", "SMITH"),
                new (caseId, 432, "Bob", "Jackson"),
            },
        };

        this.apiClientMock
            .Setup(x => x.GetCaseWitnessesAsync(It.IsAny<GetCaseWitnessesRequest>(), cmsAuthValues))
            .ReturnsAsync(expectedWitnesses);

        // Act
        var result = await this.sutWitnessService.GetCaseWitnessesAsync(caseId, cmsAuthValues);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<WitnessesResponse>(result);
        Assert.Equal(2, result.Witnesses?.Count);
        Assert.Equal(expectedWitnesses.Witnesses.First().CaseId, result.Witnesses?.First().CaseId);
        Assert.Equal(expectedWitnesses.Witnesses.First().WitnessId, result.Witnesses?.First().WitnessId);
        Assert.Equal(expectedWitnesses.Witnesses.First().FirstName, result.Witnesses?.First().FirstName);
        Assert.Equal(expectedWitnesses.Witnesses.First().Surname, result.Witnesses?.First().Surname);

        Assert.Contains(this.mockLogger.Logs, log =>
           log.LogLevel == LogLevel.Information &&
           log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Getting witnesses for caseId [{caseId}]"));
    }

    /// <summary>
    /// Tests method throws exception and logs error when an API call fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task GetWitnessesForCaseAsync_ThrowsException_WhenApiCallFails()
    {
        // Arrange
        int caseId = 4321;
        var cmsAuthValues = new CmsAuthValues("validCmsToken", "validCmsCookies");

        var expectedWitnesses = new WitnessesResponse
        {
            Witnesses = new List<Witness>()
            {
                new (caseId, 343, "Joe", "SMITH"),
                new (caseId, 432, "Bob", "Jackson"),
            },
        };

        this.apiClientMock
            .Setup(x => x.GetCaseWitnessesAsync(It.IsAny<GetCaseWitnessesRequest>(), cmsAuthValues))
            .ThrowsAsync(new Exception("DDEI-EAS API error."));

        // Act
        Exception exception = await Assert.ThrowsAsync<Exception>(() => this.sutWitnessService.GetCaseWitnessesAsync(caseId, cmsAuthValues));

        Assert.Contains(this.mockLogger.Logs, log =>
           log.LogLevel == LogLevel.Information &&
           log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Getting witnesses for caseId [{caseId}]"));

        Assert.Equal("DDEI-EAS API error.", exception.Message);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching witnesses for caseId [{caseId}]"));
    }

    /// <summary>
    /// Test GetStatementsForWitnessAsync method successfully gets witness statements.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task GetStatementsForWitnessAsync_ReturnsWitnessStatements_WhenApiCallIsSuccessful()
    {
        // Arrange
        int witnessId = 789;
        var cmsAuthValues = new CmsAuthValues("validCmsToken", "validCmsCookies");

        // Statements
        var statement1 = new WitnessStatement(
                Id: 7,
                StatementNumber: 4);

        var statement2 = new WitnessStatement(
                Id: 8,
                StatementNumber: 6);

        var statements = new List<WitnessStatement>
        {
            statement1,
            statement2,
        };
        var expectedstatements = new WitnessStatementsResponse
        {
            WitnessStatements = statements,
        };

        this.apiClientMock
            .Setup(x => x.GetWitnessStatementsAsync(It.IsAny<GetWitnessStatementsRequest>(), cmsAuthValues))
            .ReturnsAsync(expectedstatements);

        // Act
        var result = await this.sutWitnessService.GetWitnessStatementsAsync(witnessId, cmsAuthValues);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<WitnessStatementsResponse>(result);
        Assert.Equal(2, result.WitnessStatements?.Count);
        Assert.Equal(expectedstatements.WitnessStatements.First().Id, result.WitnessStatements?.First().Id);
        Assert.Equal(expectedstatements.WitnessStatements.First().StatementNumber, result.WitnessStatements?.First().StatementNumber);

        Assert.Contains(this.mockLogger.Logs, log =>
           log.LogLevel == LogLevel.Information &&
           log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Getting statements for witnessId [{witnessId}]"));
    }

    /// <summary>
    /// Test GetStatementsForWitnessAsync method successfully gets witness statements.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task GetStatementsForWitnessAsync_ThrowsExceptionAndLogsError_WhenApiCallFails()
    {
        // Arrange
        int witnessId = 789;
        var cmsAuthValues = new CmsAuthValues("validCmsToken", "validCmsCookies");

        this.apiClientMock
            .Setup(x => x.GetWitnessStatementsAsync(It.IsAny<GetWitnessStatementsRequest>(), cmsAuthValues))
             .ThrowsAsync(new Exception("DDEI-EAS API error."));

        // Act
        Exception exception = await Assert.ThrowsAsync<Exception>(() => this.sutWitnessService.GetWitnessStatementsAsync(witnessId, cmsAuthValues));

        // Assert
        Assert.Contains(this.mockLogger.Logs, log =>
           log.LogLevel == LogLevel.Information &&
           log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Getting statements for witnessId [{witnessId}]"));

        Assert.Equal("DDEI-EAS API error.", exception.Message);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching statements for witnessId [{witnessId}]"));
    }

    /// <summary>
    /// Tests method throws exception and logs error when an API call fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task AddWitnessAsync_ThrowsException_WhenApiCallFails()
    {
        // Arrange
        int caseId = 4321;
        string urn = "12345671";
        var cmsAuthValues = new CmsAuthValues("validCmsToken", "validCmsCookies");

        this.apiClientMock
            .Setup(x => x.AddWitnessAsync(It.IsAny<AddWitnessRequest>(), It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new Exception("DDEI-EAS API error."));

        this.caseLockServiceMock
           .Setup(x => x.CheckCaseLockAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
           .ReturnsAsync(new CaseLockedStatusResult
           {
               IsLocked = false,
           });

        // Act
        Exception exception = await Assert.ThrowsAsync<Exception>(() => this.sutWitnessService.AddWitnessAsync(urn, caseId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()));

        Assert.Contains(this.mockLogger.Logs, log =>
           log.LogLevel == LogLevel.Information &&
           log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Attempting to add witness to case with a caseId [{caseId}]"));

        Assert.Equal("DDEI-EAS API error.", exception.Message);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] failed to add a new witness"));
    }

    /// <summary>
    /// Tests method throws InvalidOperationException when new witness details not found.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [Fact]
    public async Task AddWitnessAsync_WhenNewWitnessDetailsNotFound_ThrowsAndLogsException()
    {
        // Arrange
        int caseId = 4321;
        var expectedWitnesses = new WitnessesResponse
        {
            Witnesses = new List<Witness>()
            {
                new (caseId, 432, "Bob", "Jackson"),
            },
        };
        var cmsAuthValues = new CmsAuthValues("validCmsToken", "validCmsCookies");

        this.apiClientMock
            .Setup(x => x.AddWitnessAsync(It.IsAny<AddWitnessRequest>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(It.IsAny<NoContentResult>());

        this.apiClientMock
            .Setup(x => x.GetCaseWitnessesAsync(It.IsAny<GetCaseWitnessesRequest>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(expectedWitnesses);

        this.caseLockServiceMock
           .Setup(x => x.CheckCaseLockAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
           .ReturnsAsync(new CaseLockedStatusResult
           {
               IsLocked = false,
           });

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => this.sutWitnessService.AddWitnessAsync(It.IsAny<string>(), caseId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CmsAuthValues>(), It.IsAny<Guid>()));

        Assert.IsType<InvalidOperationException>(exception);

        Assert.Contains(this.mockLogger.Logs, log =>
           log.LogLevel == LogLevel.Information &&
           log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Attempting to add witness to case with a caseId [{caseId}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
          log.LogLevel == LogLevel.Error &&
          log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Error occurred while adding witness for caseId [{caseId}]"));
    }

    /// <summary>
    /// Tests method succeeds when api operation is successful.
    /// </summary>
    /// <param name="firstName">The first name of the witness being added.</param>
    /// <param name="lastName">The last name of the witness being added.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    [InlineData("joe", "smith")]
    [InlineData("Joe", "smith")]
    [InlineData("JOE", "SMITH")]
    [InlineData("joe", "SMITH")]
    [InlineData("joE", "smitH")]
    [Theory]
    public async Task AddWitnessAsyncReturnsOk_WhenApiCallSucceeds(string firstName, string lastName)
    {
        // Arrange
        string urn = "123456789";
        int caseId = 4321;
        int newWitnessId = 3435;

        var expectedWitnesses = new WitnessesResponse
        {
            Witnesses = new List<Witness>()
            {
                new (caseId, newWitnessId, firstName, lastName),
                new (caseId, 432, "Bob", "Jackson"),
            },
        };
        var cmsAuthValues = new CmsAuthValues("validCmsToken", "validCmsCookies");

        this.apiClientMock
            .Setup(x => x.AddWitnessAsync(It.IsAny<AddWitnessRequest>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(It.IsAny<NoContentResult>());

        this.apiClientMock
            .Setup(x => x.GetCaseWitnessesAsync(It.IsAny<GetCaseWitnessesRequest>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(expectedWitnesses);

        this.caseLockServiceMock
            .Setup(x => x.CheckCaseLockAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(new CaseLockedStatusResult
            {
                IsLocked = false,
            });

        // Act
        int? result = await this.sutWitnessService.AddWitnessAsync(urn, caseId, "Joe", "SMITH", It.IsAny<CmsAuthValues>(), It.IsAny<Guid>());

        Assert.Contains(this.mockLogger.Logs, log =>
           log.LogLevel == LogLevel.Information &&
           log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} Attempting to add witness to case with a caseId [{caseId}]"));

        Assert.Contains(this.mockLogger.Logs, log =>
          log.LogLevel == LogLevel.Information &&
          log.Message != null && log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] successfully added a new witness, witnessId [{newWitnessId}]"));

        this.apiClientMock.Verify(x => x.AddWitnessAsync(It.IsAny<AddWitnessRequest>(), It.IsAny<CmsAuthValues>()), Times.Once());
        this.apiClientMock.Verify(x => x.GetCaseWitnessesAsync(It.IsAny<GetCaseWitnessesRequest>(), It.IsAny<CmsAuthValues>()), Times.Once());
        this.caseLockServiceMock.Verify(x => x.CheckCaseLockAsync(It.IsAny<int>(), It.IsAny<CmsAuthValues>()), Times.Once());
    }
}
