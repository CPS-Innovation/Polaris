// <copyright file="OrchestrationProviderTests.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.tests.Durable.Provider;

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

public class OrchestrationProviderTests
{
    private readonly Mock<IConfiguration> configurationMock;
    private readonly Mock<IQueryConditionFactory> queryConditionFactoryMock;
    private readonly Mock<ILogger<OrchestrationProvider>> loggerMock;
    private readonly Mock<ITelemetryClient> telemetryClientMock;
    private readonly OrchestrationProvider orchestrationProvider;

    public OrchestrationProviderTests()
    {
        this.configurationMock = new Mock<IConfiguration>();
        this.queryConditionFactoryMock = new Mock<IQueryConditionFactory>();
        this.loggerMock = new Mock<ILogger<OrchestrationProvider>>();
        this.telemetryClientMock = new Mock<ITelemetryClient>();
        this.orchestrationProvider = new OrchestrationProvider(this.configurationMock.Object, this.queryConditionFactoryMock.Object, this.loggerMock.Object, this.telemetryClientMock.Object);
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
        // arrange
        var clientMock = new Mock<DurableTaskClient>("name");
        var documentPayload = new DocumentPayload();
        var cancellationToken = CancellationToken.None;
        var existingInstance = new OrchestrationMetadata("name", "instanceId")
        {
            RuntimeStatus = orchestrationRuntimeStatus,
        };
        clientMock.Setup(s => s.GetInstanceAsync(It.IsAny<string>(), cancellationToken)).ReturnsAsync(existingInstance);

        // act
        var result = await this.orchestrationProvider.BulkSearchDocumentAsync(clientMock.Object, documentPayload, cancellationToken);

        // assert
        Assert.Equal(orchestrationProviderStatus, result.Status);
    }

    [Fact]
    public async Task BulkSearchDocumentAsync_ExistingInstanceIsNotNull_ShouldReturnFalse()
    {
        // arrange
        var clientMock = new Mock<DurableTaskClient>("name");
        var documentPayload = new DocumentPayload();
        var cancellationToken = CancellationToken.None;
        clientMock.Setup(s => s.GetInstanceAsync(It.IsAny<string>(), cancellationToken)).ReturnsAsync((OrchestrationMetadata)null);

        // act
        var result = await this.orchestrationProvider.BulkSearchDocumentAsync(clientMock.Object, documentPayload, cancellationToken);

        // assert
        clientMock.Verify(v => v.ScheduleNewOrchestrationInstanceAsync(nameof(RefreshDocumentOrchestrator), documentPayload, It.IsAny<StartOrchestrationOptions>(), cancellationToken));
        Assert.Equal(OrchestrationProviderStatus.Initiated, result.Status);
    }
}
