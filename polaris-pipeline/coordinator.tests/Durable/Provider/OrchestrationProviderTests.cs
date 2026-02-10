using Common.Telemetry;
using coordinator.Durable.Orchestration;
using coordinator.Durable.Payloads;
using coordinator.Durable.Providers;
using coordinator.Enums;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace coordinator.tests.Durable.Provider;

public class OrchestrationProviderTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IQueryConditionFactory> _queryConditionFactoryMock;
    private readonly Mock<ILogger<OrchestrationProvider>> _loggerMock;
    private readonly Mock<ITelemetryClient> _telemetryClientMock;
    private readonly OrchestrationProvider _orchestrationProvider;

    public OrchestrationProviderTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _queryConditionFactoryMock = new Mock<IQueryConditionFactory>();
        _loggerMock = new Mock<ILogger<OrchestrationProvider>>();
        _telemetryClientMock = new Mock<ITelemetryClient>();
        _orchestrationProvider = new OrchestrationProvider(_configurationMock.Object, _queryConditionFactoryMock.Object, _loggerMock.Object, _telemetryClientMock.Object);
    }

    [Theory]
    [InlineData(OrchestrationRuntimeStatus.Running, OrchestrationProviderStatus.Processing)]
    [InlineData(OrchestrationRuntimeStatus.Pending, OrchestrationProviderStatus.Processing)]
    [InlineData(OrchestrationRuntimeStatus.Suspended, OrchestrationProviderStatus.Processing)]
    [InlineData(OrchestrationRuntimeStatus.Failed, OrchestrationProviderStatus.Failed)]
    [InlineData(OrchestrationRuntimeStatus.Completed, OrchestrationProviderStatus.Completed)]
    [InlineData(OrchestrationRuntimeStatus.Terminated, OrchestrationProviderStatus.Completed)]
    public async Task BulkSearchDocumentAsync_ExistingInstanceHasInProcessStatus_ShouldReturnFalse(OrchestrationRuntimeStatus orchestrationRuntimeStatus, OrchestrationProviderStatus orchestrationProviderStatus)
    {
        //arrange
        var clientMock = new Mock<DurableTaskClient>("name");
        var documentPayload = new DocumentPayload();
        var cancellationToken = CancellationToken.None;
        var existingInstance = new OrchestrationMetadata("name", "instanceId")
        {
            RuntimeStatus = orchestrationRuntimeStatus
        };
        clientMock.Setup(s => s.GetInstanceAsync(It.IsAny<string>(), cancellationToken)).ReturnsAsync(existingInstance);

        //act
        var result = await _orchestrationProvider.BulkSearchDocumentAsync(clientMock.Object, documentPayload, cancellationToken);

        //assert
        Assert.Equal(result, orchestrationProviderStatus);
    }

    [Fact]
    public async Task BulkSearchDocumentAsync_ExistingInstanceIsNotNull_ShouldReturnFalse()
    {
        //arrange
        var clientMock = new Mock<DurableTaskClient>("name");
        var documentPayload = new DocumentPayload();
        var cancellationToken = CancellationToken.None;
        clientMock.Setup(s => s.GetInstanceAsync(It.IsAny<string>(), cancellationToken)).ReturnsAsync((OrchestrationMetadata)null);

        //act
        var result = await _orchestrationProvider.BulkSearchDocumentAsync(clientMock.Object, documentPayload, cancellationToken);

        //assert
        clientMock.Verify(v => v.ScheduleNewOrchestrationInstanceAsync(nameof(RefreshDocumentOrchestrator), documentPayload, It.IsAny<StartOrchestrationOptions>(), cancellationToken));
        Assert.Equal(result, OrchestrationProviderStatus.Initiated);
    }
}