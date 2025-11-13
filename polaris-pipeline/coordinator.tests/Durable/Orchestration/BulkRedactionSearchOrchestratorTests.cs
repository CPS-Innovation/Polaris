using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Dto.Request.Redaction;
using Common.Telemetry;
using coordinator.Domain;
using coordinator.Durable.Activity;
using coordinator.Durable.Orchestration;
using coordinator.Durable.Payloads;
using Microsoft.DurableTask;
using Moq;
using Xunit;

namespace coordinator.tests.Durable.Orchestration;

public class BulkRedactionSearchOrchestratorTests
{
    private readonly Mock<ITelemetryClient> _telemetryClientMock;
    private readonly BulkRedactionSearchOrchestrator _bulkRedactionSearchOrchestrator;
    public BulkRedactionSearchOrchestratorTests()
    {
        _telemetryClientMock = new Mock<ITelemetryClient>();
        _bulkRedactionSearchOrchestrator = new BulkRedactionSearchOrchestrator(_telemetryClientMock.Object);
    }

    [Fact]
    public async Task RunOrchestrator_OcrBlobExists_ShouldReturnRedactionDefinitionDtos()
    {
        //arrange
        var initiateOcrResponse = new InitiateOcrResponse()
        {
            BlobAlreadyExists = true
        };
        var payload = new BulkRedactionSearchPayload();
        var redactionDefinitionDtos = new List<RedactionDefinitionDto>();
        var contextMock = new Mock<TaskOrchestrationContext>();
        contextMock.Setup(s => s.GetInput<BulkRedactionSearchPayload>()).Returns(payload);
        contextMock.Setup(s => s.CallActivityAsync<InitiateOcrResponse>(nameof(InitiateOcr), It.IsAny<DocumentPayload>(), It.IsAny<TaskOptions>())).ReturnsAsync(initiateOcrResponse);
        contextMock.Setup(s => s.CallActivityAsync<IEnumerable<RedactionDefinitionDto>>(nameof(BulkRedactionSearchActivity), payload, null)).ReturnsAsync(redactionDefinitionDtos);
        //act
        var result = await _bulkRedactionSearchOrchestrator.RunOrchestrator(contextMock.Object);

        //assert
        contextMock.Verify(s => s.CallActivityAsync(nameof(SetBulkRedactionSearchStatus), It.IsAny<BulkRedactionSearchStatusPayload>(), null), Times.Exactly(3));
        Assert.Same(redactionDefinitionDtos, result);
    }
}