using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoFixture;
using Common.Services.CaseSearchService.Contracts;
using Common.Telemetry.Contracts;
using Common.Wrappers;
using Common.Wrappers.Contracts;
using coordinator.Domain;
using coordinator.Functions.Orchestration.Client.Case;
using coordinator.Functions.Orchestration.Functions.Case;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Functions
{
    public class CoordinatorStartTests
    {
        private readonly Fixture _fixture;
        private readonly string _caseUrn;
        private readonly int _caseIdNum;
        private readonly string _caseId;
        private readonly string _instanceId;
        private readonly HttpRequestMessage _httpRequestMessage;
        private readonly HttpRequestHeaders _httpRequestHeaders;
        private readonly HttpResponseMessage _httpResponseMessage;

        private readonly Mock<IDurableOrchestrationClient> _mockDurableOrchestrationClient;
        private readonly Mock<ICaseSearchClient> _mockCaseSearchClient;
        private readonly Mock<ILogger<CaseClient>> _mockLogger;
        private readonly Mock<ITelemetryClient> _mockTelemetryClient;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;

        private readonly CaseClient _coordinatorStart;

        public CoordinatorStartTests()
        {
            _fixture = new Fixture();
            _caseUrn = _fixture.Create<string>();
            _caseIdNum = _fixture.Create<int>();
            _caseId = _caseIdNum.ToString();
            var cmsAuthValues = _fixture.Create<string>();
            var correlationId = _fixture.Create<Guid>();
            _instanceId = RefreshCaseOrchestrator.GetKey(_caseId);
            _httpRequestMessage = new HttpRequestMessage();
            _jsonConvertWrapper = new JsonConvertWrapper();

            _httpRequestMessage.Method = HttpMethod.Post;
            _httpRequestMessage.RequestUri = new Uri("https://www.test.co.uk");
            _httpRequestHeaders = _httpRequestMessage.Headers;
            _httpResponseMessage = new HttpResponseMessage();

            _mockDurableOrchestrationClient = new Mock<IDurableOrchestrationClient>();
            _mockCaseSearchClient = new Mock<ICaseSearchClient>();
            _mockLogger = new Mock<ILogger<CaseClient>>();
            _mockTelemetryClient = new Mock<ITelemetryClient>();

            _httpRequestHeaders.Add("Correlation-Id", correlationId.ToString());
            _httpRequestHeaders.Add("cms-auth-values", cmsAuthValues);

            _mockDurableOrchestrationClient.Setup(client => client.GetStatusAsync(_instanceId, false, false, true))
               .ReturnsAsync(default(DurableOrchestrationStatus));

            _mockDurableOrchestrationClient.Setup(client => client.CreateCheckStatusResponse(_httpRequestMessage, _instanceId, false))
                .Returns(_httpResponseMessage);

            _coordinatorStart = new CaseClient(_mockCaseSearchClient.Object,
                                               _jsonConvertWrapper,
                                               _mockLogger.Object,
                                               _mockTelemetryClient.Object);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenCorrelationIdIsMissing()
        {
            _httpRequestHeaders.Clear();

            var httpResponseMessage = await _coordinatorStart.Run(_httpRequestMessage, _caseUrn, _caseId, _mockDurableOrchestrationClient.Object);

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenInvalidCaseUrn()
        {
            var httpResponseMessage = await _coordinatorStart.Run(_httpRequestMessage, "", _caseId, _mockDurableOrchestrationClient.Object);

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenInvalidCaseId()
        {
            var httpResponseMessage = await _coordinatorStart.Run(_httpRequestMessage, _caseUrn, "invalid", _mockDurableOrchestrationClient.Object);

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Run_ReturnsInternalServerErrorWhenUnhandledErrorOccurs()
        {
            _mockDurableOrchestrationClient.Setup(client => client.StartNewAsync(nameof(RefreshCaseOrchestrator), _instanceId, It.IsAny<CaseOrchestrationPayload>()))
                .Throws(new Exception());

            var httpResponseMessage = await _coordinatorStart.Run(_httpRequestMessage, _caseUrn, _caseId, _mockDurableOrchestrationClient.Object);

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Run_StartsOrchestratorWhenOrchestrationStatusIsNull()
        {
            // Arrange
            _mockDurableOrchestrationClient
                .Setup(client => client.GetStatusAsync(_instanceId, false, false, true))
                .ReturnsAsync(default(DurableOrchestrationStatus));

            // Act
            await _coordinatorStart.Run(_httpRequestMessage, _caseUrn, _caseId, _mockDurableOrchestrationClient.Object);

            // Assert
            _mockDurableOrchestrationClient.Verify(
                client => client.StartNewAsync(
                    nameof(RefreshCaseOrchestrator),
                    _instanceId,
                    It.Is<CaseOrchestrationPayload>(p => p.CmsCaseId == _caseIdNum)));
        }

        [Theory]
        [InlineData(OrchestrationRuntimeStatus.Completed)]
        [InlineData(OrchestrationRuntimeStatus.Terminated)]
        [InlineData(OrchestrationRuntimeStatus.Failed)]
        [InlineData(OrchestrationRuntimeStatus.Canceled)]
        public async Task Run_StartsOrchestratorWhenOrchestrationHasConcluded(OrchestrationRuntimeStatus runtimeStatus)
        {
            _mockDurableOrchestrationClient.Setup(client => client.GetStatusAsync(_instanceId, false, false, true))
               .ReturnsAsync(new DurableOrchestrationStatus { RuntimeStatus = runtimeStatus });
            await _coordinatorStart.Run(_httpRequestMessage, _caseUrn, _caseId, _mockDurableOrchestrationClient.Object);

            _mockDurableOrchestrationClient.Verify(
                client => client.StartNewAsync(
                    nameof(RefreshCaseOrchestrator),
                    _instanceId,
                    It.Is<CaseOrchestrationPayload>(p => p.CmsCaseId == _caseIdNum)));
        }

        [Fact]
        public async Task Run_LogsAtLeastOnce()
        {
            await _coordinatorStart.Run(_httpRequestMessage, _caseUrn, _caseId, _mockDurableOrchestrationClient.Object);

            _mockLogger.Verify(x => x.IsEnabled(LogLevel.Information), Times.AtLeastOnce);
        }

        [Fact]
        public async Task Run_DoesNotStartOrchestratorWhenOrchestrationHasNotConcluded()
        {
            var notStartingRuntimeStatuses = Enum.GetValues(typeof(OrchestrationRuntimeStatus))
                .Cast<OrchestrationRuntimeStatus>()
                .Except(new[] {
                    OrchestrationRuntimeStatus.Completed,
                    OrchestrationRuntimeStatus.Terminated,
                    OrchestrationRuntimeStatus.Failed,
                    OrchestrationRuntimeStatus.Canceled
                });

            foreach (var runtimeStatus in notStartingRuntimeStatuses)
            {
                _mockDurableOrchestrationClient.Setup(client => client.GetStatusAsync(_caseId, false, false, true))
                    .ReturnsAsync(new DurableOrchestrationStatus { RuntimeStatus = runtimeStatus });

                await _coordinatorStart.Run(_httpRequestMessage, _caseUrn, _caseId, _mockDurableOrchestrationClient.Object);
            }

            _mockDurableOrchestrationClient.Verify(
                client => client.StartNewAsync(
                    nameof(RefreshCaseOrchestrator),
                    _caseId,
                    It.Is<CaseOrchestrationPayload>(p => p.CmsCaseId == _caseIdNum)),
                Times.Never);
        }

        [Fact]
        public async Task Run_ReturnsExpectedHttpResponseMessage()
        {
            // Act
            var httpResponseMessage = await _coordinatorStart.Run(_httpRequestMessage, _caseUrn, _caseId, _mockDurableOrchestrationClient.Object);

            // Assert
            httpResponseMessage.Should().Be(_httpResponseMessage);
        }
    }
}
