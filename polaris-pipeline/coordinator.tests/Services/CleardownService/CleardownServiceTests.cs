using System;
using AutoFixture;
using Common.Configuration;
using Moq;
using Xunit;
using Common.Dto.Response;
using Common.Services.BlobStorage;
using Common.Telemetry;
using coordinator.Clients.TextExtractor;
using coordinator.Durable.Providers;
using coordinator.Services.ClearDownService;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;

namespace coordinator.tests.Services.CleardownServiceTests
{
  public class ClearDownServiceTests
  {
    private readonly string _caseUrn;
    private readonly int _caseId;
    private readonly Guid _correlationId;
    private readonly Mock<IPolarisBlobStorageService> _mockBlobStorageService;
    private readonly Mock<ITextExtractorClient> _mockTextExtractorClient;
    private readonly Mock<IOrchestrationProvider> _mockOrchestrationProvider;
    private readonly Mock<ITelemetryClient> _mockTelemetryClient;
    private readonly Mock<IDurableOrchestrationClient> _mockDurableOrchestrationClient;

    private readonly IClearDownService _clearDownService;

    public ClearDownServiceTests()
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

      var mockConfiguration = new Mock<IConfiguration>();
      mockConfiguration.Setup(x => x[StorageKeys.BlobServiceContainerNameDocuments]).Returns("Documents");

      var mockStorageDelegate = new Mock<Func<string, IPolarisBlobStorageService>>();
      mockStorageDelegate.Setup(s => s("Documents")).Returns(_mockBlobStorageService.Object);

      _mockOrchestrationProvider = new Mock<IOrchestrationProvider>();
      _mockTelemetryClient = new Mock<ITelemetryClient>();
      _clearDownService = new ClearDownService(mockStorageDelegate.Object, _mockTextExtractorClient.Object, _mockOrchestrationProvider.Object, _mockTelemetryClient.Object, mockConfiguration.Object);
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
      _clearDownService.DeleteCaseAsync(_mockDurableOrchestrationClient.Object, _caseUrn, _caseId, _correlationId);

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
      _clearDownService.DeleteCaseAsync(_mockDurableOrchestrationClient.Object, _caseUrn, _caseId, _correlationId);

      // Assert
      _mockTelemetryClient.Verify(m => m.TrackEvent(It.IsAny<BaseTelemetryEvent>()), Times.Never);
    }
  }
}