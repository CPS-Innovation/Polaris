// <copyright file="CleardownServiceTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.tests.Services.CleardownServiceTests;

using System;
using AutoFixture;
using Common.Configuration;
using Moq;
using Xunit;
using Common.Dto.Response;
using Common.Services.BlobStorage;
using coordinator.Clients.TextExtractor;
using coordinator.Durable.Providers;
using coordinator.Services.ClearDownService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.DurableTask.Client;
using System.Threading.Tasks;
using FluentAssertions;

public class ClearDownServiceTests
{
    private readonly string caseUrn;
    private readonly int caseId;
    private readonly Guid correlationId;
    private readonly Mock<IPolarisBlobStorageService> mockBlobStorageService;
    private readonly Mock<ITextExtractorClient> mockTextExtractorClient;
    private readonly Mock<IOrchestrationProvider> mockOrchestrationProvider;
    private readonly Mock<ILogger<ClearDownService>> mockLogger;
    private readonly Mock<DurableTaskClient> mockDurableOrchestrationClient;

    private readonly ClearDownService clearDownService;

    public ClearDownServiceTests()
    {
        var fixture = new Fixture();
        this.caseId = fixture.Create<int>();
        this.caseUrn = fixture.Create<string>();
        this.correlationId = fixture.Create<Guid>();

        this.mockDurableOrchestrationClient = new Mock<DurableTaskClient>("name");
        this.mockBlobStorageService = new Mock<IPolarisBlobStorageService>();
        this.mockTextExtractorClient = new Mock<ITextExtractorClient>();
        this.mockTextExtractorClient.Setup(m => m.RemoveCaseIndexesAsync(this.caseUrn, this.caseId, this.correlationId))
          .ReturnsAsync(new IndexDocumentsDeletedResult());

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(x => x[StorageKeys.BlobServiceContainerNameDocuments]).Returns("Documents");

        var mockStorageDelegate = new Mock<Func<string, IPolarisBlobStorageService>>();
        mockStorageDelegate.Setup(s => s("Documents")).Returns(this.mockBlobStorageService.Object);

        this.mockOrchestrationProvider = new Mock<IOrchestrationProvider>();
        this.mockLogger = new Mock<ILogger<ClearDownService>>();
        this.clearDownService = new ClearDownService(mockStorageDelegate.Object, this.mockTextExtractorClient.Object, this.mockOrchestrationProvider.Object, this.mockLogger.Object, mockConfiguration.Object);
    }

    [Fact]
    public async Task DeleteCaseAsync_CallTrackEventWhenOrchestrationResultIsSuccessTrueAsync()
    {
        // Arrange
        var orchestrationResult = new DeleteCaseOrchestrationResult
        {
            IsSuccess = true,
        };
        this.mockOrchestrationProvider.Setup(m => m.DeleteCaseOrchestrationAsync(this.mockDurableOrchestrationClient.Object, this.caseId))
          .ReturnsAsync(orchestrationResult);

        // Act
        await this.clearDownService.DeleteCaseAsync(this.mockDurableOrchestrationClient.Object, this.caseUrn, this.caseId, this.correlationId);

        // Assert
        this.mockLogger.Verify(
            m => m.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task DeleteCaseAsync_NotCallTrackEventWhenOrchestrationResultIsSuccessFalseAsync()
    {
        // Arrange
        var orchestrationResult = new DeleteCaseOrchestrationResult
        {
            IsSuccess = false,
        };

        this.mockOrchestrationProvider.Setup(m => m.DeleteCaseOrchestrationAsync(this.mockDurableOrchestrationClient.Object, this.caseId))
          .ReturnsAsync(orchestrationResult);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => this.clearDownService.DeleteCaseAsync(this.mockDurableOrchestrationClient.Object, this.caseUrn, this.caseId, this.correlationId));

        // Assert
        exception.Message.Should().Be($"Error deleting case {this.caseId}");
        exception.InnerException.Should().NotBeNull();
        exception.InnerException.Message.Should().Be("DeleteCaseOrchestrationAsync failed");
    }
}
