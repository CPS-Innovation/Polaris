using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AutoFixture;
using Common.Services.BlobStorageService.Contracts;
using coordinator.Domain;
using coordinator.Functions.Orchestration.Client.Case;
using coordinator.Functions.Orchestration.Functions.Case;
using coordinator.Providers;
using coordinator.Services.CleardownService;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Functions
{
    public class CoordinatorStartTests
    {
        private readonly string _caseUrn;
        private readonly int _caseIdNum;
        private readonly string _caseId;
        private readonly string _instanceId;
        private readonly Guid _correlationId;
        private readonly HttpRequestMessage _httpRequestMessage;
        private readonly HttpRequestHeaders _httpRequestHeaders;
        private readonly HttpResponseMessage _httpResponseMessage;

        private readonly Mock<IDurableOrchestrationClient> _mockDurableOrchestrationClient;
        private readonly Mock<IOrchestrationProvider> _mockOrchestrationProvider;
        private readonly Mock<ICleardownService> _mockCleardownService;

        private readonly CaseClient _coordinatorStart;

        public CoordinatorStartTests()
        {
            var fixture = new Fixture();
            _caseUrn = fixture.Create<string>();
            _caseIdNum = fixture.Create<int>();
            _caseId = _caseIdNum.ToString();
            var cmsAuthValues = fixture.Create<string>();
            _correlationId = fixture.Create<Guid>();
            _instanceId = RefreshCaseOrchestrator.GetKey(_caseId);
            _httpRequestMessage = new HttpRequestMessage();

            _httpRequestMessage.Method = HttpMethod.Post;
            _httpRequestMessage.RequestUri = new Uri("https://www.test.co.uk");
            _httpRequestHeaders = _httpRequestMessage.Headers;
            _httpResponseMessage = new HttpResponseMessage();

            _mockDurableOrchestrationClient = new Mock<IDurableOrchestrationClient>();
            var mockLogger = new Mock<ILogger<CaseClient>>();
            var mockBlobStorageClient = new Mock<IPolarisBlobStorageService>();
            _mockOrchestrationProvider = new Mock<IOrchestrationProvider>();
            _mockCleardownService = new Mock<ICleardownService>();
            
            _httpRequestHeaders.Add("Correlation-Id", _correlationId.ToString());
            _httpRequestHeaders.Add("cms-auth-values", cmsAuthValues);

            mockBlobStorageClient.Setup(s => s.DeleteBlobsByCaseAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _mockDurableOrchestrationClient.Setup(client => client.GetStatusAsync(_instanceId, false, false, true))
               .ReturnsAsync(default(DurableOrchestrationStatus));

            _mockDurableOrchestrationClient.Setup(client => client.CreateCheckStatusResponse(_httpRequestMessage, _instanceId, false))
                .Returns(_httpResponseMessage);

            _mockOrchestrationProvider.Setup(s => s.RefreshCaseAsync(_mockDurableOrchestrationClient.Object,
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CaseOrchestrationPayload>(), _httpRequestMessage))
                .ReturnsAsync(_httpResponseMessage);
            _mockOrchestrationProvider.Setup(s => s.DeleteCaseOrchestrationAsync(_mockDurableOrchestrationClient.Object,
                    It.IsAny<int>()));

            _mockCleardownService.Setup(s => s.DeleteCaseAsync(_mockDurableOrchestrationClient.Object,
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<bool>()));
            _coordinatorStart = new CaseClient(mockLogger.Object, _mockOrchestrationProvider.Object, _mockCleardownService.Object);
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
            _mockOrchestrationProvider.Setup(s => s.RefreshCaseAsync(_mockDurableOrchestrationClient.Object,
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CaseOrchestrationPayload>(), _httpRequestMessage))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

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
            _mockOrchestrationProvider.Verify(
                client => client.RefreshCaseAsync(
                    _mockDurableOrchestrationClient.Object,
                    _correlationId,
                    _caseId,
                    It.IsAny<CaseOrchestrationPayload>(),
                    _httpRequestMessage));
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

            _mockOrchestrationProvider.Verify(
                client => client.RefreshCaseAsync(
                    _mockDurableOrchestrationClient.Object,
                    _correlationId,
                    _caseId,
                    It.Is<CaseOrchestrationPayload>(p => p.CmsCaseId == _caseIdNum),
                    _httpRequestMessage));
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