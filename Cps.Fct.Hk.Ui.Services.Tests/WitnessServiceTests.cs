// <copyright file="WitnessServiceTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services.Tests;

using Common.Constants;
using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response;
using Common.Dto.Response.HouseKeeping;
using Cps.Fct.Hk.Ui.Interfaces;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using DdeiClient.Clients.Interfaces;
using DdeiClient.Domain.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        var cmsAuthValues = new CmsAuthValues("validCmsToken");

        IEnumerable<BaseCaseWitnessResponse> apiResponse =
        [
            new MdsCaseWitnessResponse
        {
            Id = 343,
            FirstName = "Joe",
            Surname = "SMITH",
        },
        new MdsCaseWitnessResponse
        {
            Id = 432,
            FirstName = "Bob",
            Surname = "Jackson",
        },
    ];

        this.apiClientMock
            .Setup(x => x.GetCaseWitnessesAsync(
                It.IsAny<GetCaseWitnessesRequest>(),
                cmsAuthValues))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await this.sutWitnessService.GetCaseWitnessesAsync(
            caseId,
            cmsAuthValues);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<WitnessesResponse>(result);
        Assert.NotNull(result.Witnesses);
        Assert.Equal(2, result.Witnesses.Count);

        Assert.Equal(caseId, result.Witnesses[0].CaseId);
        Assert.Equal(343, result.Witnesses[0].WitnessId);
        Assert.Equal("Joe", result.Witnesses[0].FirstName);
        Assert.Equal("SMITH", result.Witnesses[0].Surname);

        Assert.Equal(caseId, result.Witnesses[1].CaseId);
        Assert.Equal(432, result.Witnesses[1].WitnessId);
        Assert.Equal("Bob", result.Witnesses[1].FirstName);
        Assert.Equal("Jackson", result.Witnesses[1].Surname);

        Assert.Contains(
            this.mockLogger.Logs,
            log =>
                log.LogLevel == LogLevel.Information &&
                log.Message != null &&
                log.Message.Contains(
                    $"{LoggingConstants.HskUiLogPrefix} Getting witnesses for caseId {caseId}"));
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
        var cmsAuthValues = new CmsAuthValues("validCmsToken");

        this.apiClientMock
            .Setup(x => x.GetCaseWitnessesAsync(
                It.IsAny<GetCaseWitnessesRequest>(),
                cmsAuthValues))
            .ThrowsAsync(new Exception("DDEI-EAS API error."));

        // Act
        Exception exception = await Assert.ThrowsAsync<Exception>(() =>
            this.sutWitnessService.GetCaseWitnessesAsync(
                caseId,
                cmsAuthValues));

        // Assert
        Assert.Contains(
            this.mockLogger.Logs,
            log =>
                log.LogLevel == LogLevel.Information &&
                log.Message != null &&
                log.Message.Contains(
                    $"{LoggingConstants.HskUiLogPrefix} Getting witnesses for caseId {caseId}"));

        Assert.Equal("DDEI-EAS API error.", exception.Message);

        Assert.Contains(
            this.mockLogger.Logs,
            log =>
                log.LogLevel == LogLevel.Error &&
                log.Message != null &&
                log.Message.Contains(
                    $"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching witnesses"));
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
        var cmsAuthValues = new CmsAuthValues("validCmsToken");

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
        var cmsAuthValues = new CmsAuthValues("validCmsToken");

        this.apiClientMock
            .Setup(x => x.GetWitnessStatementsAsync(
                It.IsAny<GetWitnessStatementsRequest>(),
                cmsAuthValues))
            .ThrowsAsync(new Exception("DDEI-EAS API error."));

        // Act
        Exception exception = await Assert.ThrowsAsync<Exception>(() =>
            this.sutWitnessService.GetWitnessStatementsAsync(
                witnessId,
                cmsAuthValues));

        // Assert
        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null &&
            log.Message.Contains(
                $"{LoggingConstants.HskUiLogPrefix} Getting statements for witnessId [{witnessId}]"));

        Assert.Equal("DDEI-EAS API error.", exception.Message);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null &&
            log.Message.Contains(
                $"{LoggingConstants.HskUiLogPrefix} Error occurred while fetching statement"));
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
        var cmsAuthValues = new CmsAuthValues("validCmsToken");

        this.apiClientMock
            .Setup(x => x.AddWitnessAsync(
                It.IsAny<AddWitnessRequest>(),
                It.IsAny<CmsAuthValues>()))
            .ThrowsAsync(new Exception("DDEI-EAS API error."));

        this.caseLockServiceMock
            .Setup(x => x.CheckCaseLockAsync(
                It.IsAny<int>(),
                It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(new CaseLockedStatusResult
            {
                IsLocked = false,
            });

        // Act
        Exception exception = await Assert.ThrowsAsync<Exception>(() =>
            this.sutWitnessService.AddWitnessAsync(
                urn,
                caseId,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CmsAuthValues>(),
                It.IsAny<Guid>()));

        // Assert
        Assert.Equal("DDEI-EAS API error.", exception.Message);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null &&
            log.Message.Contains("Attempting to add witness to case"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null &&
            log.Message.Contains(
                $"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] failed to add a new witness"));
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

        IEnumerable<BaseCaseWitnessResponse> apiResponse =
        [
            new MdsCaseWitnessResponse
        {
            Id = 432,
            FirstName = "Bob",
            Surname = "Jackson",
        },
    ];

        var cmsAuthValues = new CmsAuthValues("validCmsToken");

        this.apiClientMock
            .Setup(x => x.AddWitnessAsync(
                It.IsAny<AddWitnessRequest>(),
                It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(It.IsAny<NoContentResult>());

        this.apiClientMock
            .Setup(x => x.GetCaseWitnessesAsync(
                It.IsAny<GetCaseWitnessesRequest>(),
                It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(apiResponse);

        this.caseLockServiceMock
            .Setup(x => x.CheckCaseLockAsync(
                It.IsAny<int>(),
                It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(new CaseLockedStatusResult
            {
                IsLocked = false,
            });

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            this.sutWitnessService.AddWitnessAsync(
                It.IsAny<string>(),
                caseId,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CmsAuthValues>(),
                It.IsAny<Guid>()));

        // Assert
        Assert.Equal(
            $"{LoggingConstants.HskUiLogPrefix} Error occurred while adding witness for caseId [{caseId}]",
            exception.Message);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Information &&
            log.Message != null &&
            log.Message.Contains("Attempting to add witness to case"));

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null &&
            log.Message.Contains(
                $"{LoggingConstants.HskUiLogPrefix} caseId [{caseId}] failed to add a new witness"));
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
    public async Task AddWitnessAsyncReturnsOk_WhenApiCallSucceeds(
    string firstName,
    string lastName)
    {
        // Arrange
        string urn = "123456789";
        int caseId = 4321;
        int newWitnessId = 3435;

        var cmsAuthValues = new CmsAuthValues("validCmsToken");
        var correspondenceId = Guid.NewGuid();

        IEnumerable<BaseCaseWitnessResponse> apiResponse =
        [
            new MdsCaseWitnessResponse
        {
            Id = newWitnessId,
            FirstName = firstName,
            Surname = lastName,
        },
        new MdsCaseWitnessResponse
        {
            Id = 432,
            FirstName = "Bob",
            Surname = "Jackson",
        },
    ];

        this.apiClientMock
            .Setup(x => x.AddWitnessAsync(
                It.IsAny<AddWitnessRequest>(),
                It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(It.IsAny<NoContentResult>());

        this.apiClientMock
            .Setup(x => x.GetCaseWitnessesAsync(
                It.IsAny<GetCaseWitnessesRequest>(),
                It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(apiResponse);

        this.caseLockServiceMock
            .Setup(x => x.CheckCaseLockAsync(
                It.IsAny<int>(),
                It.IsAny<CmsAuthValues>()))
            .ReturnsAsync(new CaseLockedStatusResult
            {
                IsLocked = false,
            });

        // Act
        long? result = await this.sutWitnessService.AddWitnessAsync(
            urn,
            caseId,
            "Joe",
            "SMITH",
            cmsAuthValues,
            correspondenceId);

        // Assert
        Assert.Equal(newWitnessId, result);

        Assert.Contains(
            this.mockLogger.Logs,
            log =>
                log.LogLevel == LogLevel.Information &&
                log.Message != null &&
                log.Message.Contains("Attempting to add witness to case"));

        Assert.Contains(
            this.mockLogger.Logs,
            log =>
                log.LogLevel == LogLevel.Information &&
                log.Message != null &&
                log.Message.Contains($"caseId [{caseId}]") &&
                log.Message.Contains("successfully added"));

        this.apiClientMock.Verify(
            x => x.AddWitnessAsync(
                It.IsAny<AddWitnessRequest>(),
                It.IsAny<CmsAuthValues>()),
            Times.Once());

        this.apiClientMock.Verify(
            x => x.GetCaseWitnessesAsync(
                It.IsAny<GetCaseWitnessesRequest>(),
                It.IsAny<CmsAuthValues>()),
            Times.Once());

        this.caseLockServiceMock.Verify(
            x => x.CheckCaseLockAsync(
                It.IsAny<int>(),
                It.IsAny<CmsAuthValues>()),
            Times.Once());
    }
}
