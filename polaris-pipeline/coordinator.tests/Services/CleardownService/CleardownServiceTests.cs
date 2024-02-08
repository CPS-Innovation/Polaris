using Xunit;
using Moq;
using coordinator.Services.CleardownService;
using Common.Services.BlobStorageService.Contracts;
using coordinator.Clients.Contracts;
using coordinator.Providers;
using Common.Telemetry.Contracts;
using AutoFixture;
using System;
using Common.Dto.Response;
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
      _mockTextExtractorClient.Setup(m => m.RemoveCaseIndexesAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Guid>()))
        .ReturnsAsync(new IndexDocumentsDeletedResult());
      _mockOrchestrationProvider = new Mock<IOrchestrationProvider>();
      _mockTelemetryClient = new Mock<ITelemetryClient>();
      _cleardownService = new CleardownService(_mockBlobStorageService.Object, _mockTextExtractorClient.Object, _mockOrchestrationProvider.Object, _mockTelemetryClient.Object);
    }

    [Fact]
    public void DeleteCaseAsync_CallsWaitForCaseEmptyResultsAsyncWhenWaitForIndexToSettleIsTrue()
    {
      // Arrange
      var waitForIndexToSettle = true;

      // Act
      _cleardownService.DeleteCaseAsync(_mockDurableOrchestrationClient.Object, _caseUrn, _caseId, _correlationId, waitForIndexToSettle);

      // Assert
      _mockTextExtractorClient.Verify(m => m.WaitForCaseEmptyResultsAsync(_caseUrn, _caseId, _correlationId), Times.Once);
    }
  }
}