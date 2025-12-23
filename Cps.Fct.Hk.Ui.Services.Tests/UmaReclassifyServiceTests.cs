// <copyright file="UmaReclassifyServiceTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services.Tests;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Constants;
using Common.Dto.Response.HouseKeeping;
using Cps.Fct.Hk.Ui.Interfaces.Model;
using Cps.Fct.Hk.Ui.ServiceClient.Uma;
using Cps.Fct.Hk.Ui.Services.Tests.TestUtilities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for the <see cref="UmaReclassifyService"/> class.
/// </summary>
public class UmaReclassifyServiceTests
{
    private readonly TestLogger<UmaReclassifyService> mockLogger;
    private readonly Mock<IUmaServiceClient> mockUmaClient;
    private readonly UmaReclassifyService umaReclassifyService;

    /// <summary>
    /// Initializes a new instance of the <see cref="UmaReclassifyServiceTests"/> class.
    /// </summary>
    public UmaReclassifyServiceTests()
    {
        this.mockLogger = new TestLogger<UmaReclassifyService>();
        this.mockUmaClient = new Mock<IUmaServiceClient>();
        this.umaReclassifyService = new UmaReclassifyService(this.mockLogger, this.mockUmaClient.Object);
    }

    /// <summary>
    /// Verifies that <see cref="UmaReclassifyService.ProcessMatchingRequest"/> returns matched communications when valid inputs are provided.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ProcessMatchingRequest_ValidCommunications_ReturnsMatchedCommunications()
    {
        // Arrange
        var communications = new List<Communication>
        {
            new Communication(
                Id: 1,
                OriginalFileName: "file1.pdf",
                Subject: "Subject 1",
                DocumentTypeId: 1,
                MaterialId: 1001,
                Link: "http://example.com/file1",
                Status: "Pending",
                Category: "Category 1",
                Type: "Type A",
                HasAttachments: false),

            new Communication(
                Id: 2,
                OriginalFileName: "file2.pdf",
                Subject: "Subject 2",
                DocumentTypeId: 2,
                MaterialId: 1002,
                Link: "http://example.com/file2",
                Status: "Processed",
                Category: "Category 2",
                Type: "Type B",
                HasAttachments: false),
        };

        var matchedCommunications = new List<MatchedCommunication>
        {
            new MatchedCommunication { materialId = 1, subject = "Matched 1" },
            new MatchedCommunication { materialId = 2, subject = "Matched 2" },
        };

        this.mockUmaClient
            .Setup(client => client.MatchCommunicationsUmAsync(It.IsAny<int>(), communications))
            .ReturnsAsync(matchedCommunications);

        // Act
        IReadOnlyCollection<MatchedCommunication> result = await this.umaReclassifyService.ProcessMatchingRequest(It.IsAny<int>(), communications);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        this.mockUmaClient.Verify(client => client.MatchCommunicationsUmAsync(It.IsAny<int>(), communications), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="UmaReclassifyService.ProcessMatchingRequest"/> throws an exception when an exception is thrown by the client.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ProcessMatchingRequest_ClientThrowsException_LogsErrorAndThrows()
    {
        // Arrange
        var communications = new List<Communication>
        {
            new Communication(
                Id: 1,
                OriginalFileName: "file1.pdf",
                Subject: "Subject 1",
                DocumentTypeId: 1,
                MaterialId: 1001,
                Link: "http://example.com/file1",
                Status: "Pending",
                Category: "Category 1",
                Type: "Type A",
                HasAttachments: false),

            new Communication(
                Id: 2,
                OriginalFileName: "file2.pdf",
                Subject: "Subject 2",
                DocumentTypeId: 2,
                MaterialId: 1002,
                Link: "http://example.com/file2",
                Status: "Processed",
                Category: "Category 2",
                Type: "Type B",
                HasAttachments: false),
        };

        this.mockUmaClient
            .Setup(client => client.MatchCommunicationsUmAsync(It.IsAny<int>(), communications))
            .ThrowsAsync(new Exception("Simulated exception"));

        // Act & Assert
        Exception exception = await Assert.ThrowsAsync<Exception>(() =>
            this.umaReclassifyService.ProcessMatchingRequest(It.IsAny<int>(), communications));

        Assert.Equal("Simulated exception", exception.Message);

        this.mockUmaClient.Verify(client => client.MatchCommunicationsUmAsync(It.IsAny<int>(), communications), Times.Once);

        Assert.Contains(this.mockLogger.Logs, log =>
            log.LogLevel == LogLevel.Error &&
            log.Message != null &&
            log.Message.Contains($"{LoggingConstants.HskUiLogPrefix} ProcessMatchingRequest encountered an error"));
    }

    /// <summary>
    /// Verifies that <see cref="UmaReclassifyService.ProcessMatchingRequest"/> throws an <see cref="ArgumentNullException"/> when the communications parameter is null.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    [Fact]
    public async Task ProcessMatchingRequest_NullCommunications_ThrowsArgumentNullException()
    {
        // Arrange
        IReadOnlyCollection<Communication>? communications = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => this.umaReclassifyService.ProcessMatchingRequest(It.IsAny<int>(), communications!));
        this.mockUmaClient.Verify(client => client.MatchCommunicationsUmAsync(It.IsAny<int>(), It.IsAny<IReadOnlyCollection<Communication>>()), Times.Never);
    }
}
