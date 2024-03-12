using System;
using AutoFixture;
using Moq;
using Xunit;
using Common.Dto.Response;
using Common.Services.BlobStorageService;
using Common.Telemetry;
using coordinator.Clients.TextExtractor;
using coordinator.Durable.Providers;
using coordinator.Services.TextExtractService;
using coordinator.Services.CleardownService;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.tests.Services.CleardownServiceTests
{
  public class CleardownServiceTests
  {
    private readonly string _caseUrn;
    private readonly int _caseId;
    private readonly Guid _correlationId;
    private readonly Mock<IPolarisBlobStorageService> _mockBlobStorageService;
    private readonly Mock<ITextExtractorClient> _mockTextExtractorClient;
    private readonly Mock<ITextExtractService> _mockTextExtractorService;
    private readonly Mock<IOrchestrationProvider> _mockOrchestrationProvider;
    private readonly Mock<ITelemetryClient> _mockTelemetryClient;
    private readonly Mock<IDurableOrchestrationClient> _mockDurableOrchestrationClient;

    private readonly ICleardownService _cleardownService;

    public CleardownServiceTests()
    {
      var fixture = new Fixture();
      _caseId = fixture.Create<int>();
      _caseUrn = fixture.Create<string>();
      _correlationId = fixture.Create<Guid>();

      _mockDurableOrchestrationClient = new Mock<IDurableOrchestrationClient>();
      _mockBlobStorageService = new Mock<IPolarisBlobStorageService>();
      _mockTextExtractorClient = new Mock<ITextExtractorClient>();
      _mockTextExtractorClient.Setup(m => m.RemoveCaseIndexesAsync(_caseUrn, _caseId, _correlationId))
        .ReturnsAsync(new IndexDocumentsDeletedResult());
      _mockTextExtractorService = new Mock<ITextExtractService>();
      _mockTextExtractorService.Setup(m => m.WaitForCaseEmptyResultsAsync(_caseUrn, _caseId, _correlationId))
        .ReturnsAsync(new IndexSettledResult());

      _mockOrchestrationProvider = new Mock<IOrchestrationProvider>();
      _mockTelemetryClient = new Mock<ITelemetryClient>();
      _cleardownService = new CleardownService(_mockBlobStorageService.Object, _mockTextExtractorClient.Object, _mockTextExtractorService.Object, _mockOrchestrationProvider.Object, _mockTelemetryClient.Object);
    }

    [Fact]
    public void DeleteCaseAsync_CallsWaitForCaseEmptyResultsAsyncWhenWaitForIndexToSettleIsTrue()
    {
      // Arrange
      var waitForIndexToSettle = true;

      // Act
      _cleardownService.DeleteCaseAsync(_mockDurableOrchestrationClient.Object, _caseUrn, _caseId, _correlationId, waitForIndexToSettle);

      // Assert
      _mockTextExtractorService.Verify(m => m.WaitForCaseEmptyResultsAsync(_caseUrn, _caseId, _correlationId), Times.Once);
    }

    [Fact]
    public void DeleteCaseAsync_NotCallWaitForCaseEmptyResultsAsyncWhenWaitForIndexToSettleIsFalse()
    {
      // Arrange
      var waitForIndexToSettle = false;

      // Act
      _cleardownService.DeleteCaseAsync(_mockDurableOrchestrationClient.Object, _caseUrn, _caseId, _correlationId, waitForIndexToSettle);

      // Assert
      _mockTextExtractorService.Verify(m => m.WaitForCaseEmptyResultsAsync(_caseUrn, _caseId, _correlationId), Times.Never);
    }

    [Fact]
    public void DeleteCaseAsync_CallTrackEventWhenOrchestrationResultIsSuccessTrue()
    {
      // Arrange
      var orchestrationResult = new DeleteCaseOrchestrationResult
      {
        IsSuccess = true
      };
      _mockOrchestrationProvider.Setup(m => m.DeleteCaseOrchestrationAsync(_mockDurableOrchestrationClient.Object, _caseId))
        .ReturnsAsync(orchestrationResult);

      // Act
      _cleardownService.DeleteCaseAsync(_mockDurableOrchestrationClient.Object, _caseUrn, _caseId, _correlationId, false);

      // Assert
      _mockTelemetryClient.Verify(m => m.TrackEvent(It.IsAny<BaseTelemetryEvent>()), Times.Once);
    }

    [Fact]
    public void DeleteCaseAsync_NotCallTrackEventWhenOrchestrationResultIsSuccessFalse()
    {
      // Arrange
      var orchestrationResult = new DeleteCaseOrchestrationResult
      {
        IsSuccess = false
      };
      _mockOrchestrationProvider.Setup(m => m.DeleteCaseOrchestrationAsync(_mockDurableOrchestrationClient.Object, _caseId))
        .ReturnsAsync(orchestrationResult);

      // Act
      _cleardownService.DeleteCaseAsync(_mockDurableOrchestrationClient.Object, _caseUrn, _caseId, _correlationId, false);

      // Assert
      _mockTelemetryClient.Verify(m => m.TrackEvent(It.IsAny<BaseTelemetryEvent>()), Times.Never);
    }
  }
}