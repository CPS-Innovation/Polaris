using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Common.Services.BlobStorageService.Contracts;
using coordinator.Durable.Payloads;
using coordinator.Functions;
using coordinator.Durable.Orchestration;
using coordinator.Durable.Providers;
using coordinator.Services.CleardownService;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace coordinator.tests.Functions
{
    public class CoordinatorStartTests
    {
        private readonly string _caseUrn;
        private readonly int _caseId;
        private readonly string _instanceId;
        private readonly Guid _correlationId;
        private readonly HttpRequest _httpRequest;
        private readonly IHeaderDictionary _httpRequestHeaders;
        private readonly IActionResult _actionResult;
        private readonly Mock<IDurableOrchestrationClient> _mockDurableOrchestrationClient;
        private readonly Mock<IOrchestrationProvider> _mockOrchestrationProvider;
        private readonly Mock<ICleardownService> _mockCleardownService;
        private readonly RefreshCase _coordinatorStart;

        public CoordinatorStartTests()
        {
            var fixture = new Fixture();
            _caseUrn = fixture.Create<string>();
            _caseId = fixture.Create<int>();

            var cmsAuthValues = fixture.Create<string>();
            _correlationId = fixture.Create<Guid>();
            _instanceId = RefreshCaseOrchestrator.GetKey(_caseId.ToString());

            _httpRequest = new DefaultHttpContext().Request;
            _httpRequest.Method = "POST";
            _httpRequestHeaders = _httpRequest.Headers;
            _actionResult = new StatusCodeResult(200);

            _mockDurableOrchestrationClient = new Mock<IDurableOrchestrationClient>();
            var mockLogger = new Mock<ILogger<RefreshCase>>();
            var mockBlobStorageClient = new Mock<IPolarisBlobStorageService>();
            _mockOrchestrationProvider = new Mock<IOrchestrationProvider>();
            _mockCleardownService = new Mock<ICleardownService>();

            _httpRequestHeaders.Add("Correlation-Id", _correlationId.ToString());
            _httpRequestHeaders.Add("cms-auth-values", cmsAuthValues);

            mockBlobStorageClient.Setup(s => s.DeleteBlobsByCaseAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _mockDurableOrchestrationClient.Setup(client => client.GetStatusAsync(_instanceId, false, false, true))
               .ReturnsAsync(default(DurableOrchestrationStatus));

            _mockDurableOrchestrationClient.Setup(client => client.CreateCheckStatusResponse(_httpRequest, _instanceId, false))
                .Returns(_actionResult);

            _mockOrchestrationProvider.Setup(s => s.RefreshCaseAsync(_mockDurableOrchestrationClient.Object,
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CaseOrchestrationPayload>(), _httpRequest))
                .ReturnsAsync(_actionResult);
            _mockOrchestrationProvider.Setup(s => s.DeleteCaseOrchestrationAsync(_mockDurableOrchestrationClient.Object,
                    It.IsAny<int>()));

            _mockCleardownService.Setup(s => s.DeleteCaseAsync(_mockDurableOrchestrationClient.Object,
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<bool>()));
            _coordinatorStart = new RefreshCase(mockLogger.Object, _mockOrchestrationProvider.Object, _mockCleardownService.Object);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenCorrelationIdIsMissing()
        {
            _httpRequestHeaders.Clear();

            var result = await _coordinatorStart.Run(_httpRequest, _caseUrn, _caseId, _mockDurableOrchestrationClient.Object);

            (result as StatusCodeResult).StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Run_ReturnsInternalServerErrorWhenUnhandledErrorOccurs()
        {
            _mockDurableOrchestrationClient.Setup(client => client.StartNewAsync(nameof(RefreshCaseOrchestrator), _instanceId, It.IsAny<CaseOrchestrationPayload>()))
                .Throws(new Exception());
            _mockOrchestrationProvider.Setup(s => s.RefreshCaseAsync(_mockDurableOrchestrationClient.Object,
                    It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CaseOrchestrationPayload>(), _httpRequest))
                .ReturnsAsync(new StatusCodeResult(500));

            var result = await _coordinatorStart.Run(_httpRequest, _caseUrn, _caseId, _mockDurableOrchestrationClient.Object);

            (result as StatusCodeResult).StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Run_StartsOrchestratorWhenOrchestrationStatusIsNull()
        {
            // Arrange
            _mockDurableOrchestrationClient
                .Setup(client => client.GetStatusAsync(_instanceId, false, false, true))
                .ReturnsAsync(default(DurableOrchestrationStatus));

            // Act
            await _coordinatorStart.Run(_httpRequest, _caseUrn, _caseId, _mockDurableOrchestrationClient.Object);

            // Assert
            _mockOrchestrationProvider.Verify(
                client => client.RefreshCaseAsync(
                    _mockDurableOrchestrationClient.Object,
                    _correlationId,
                    _caseId.ToString(),
                    It.IsAny<CaseOrchestrationPayload>(),
                    _httpRequest));
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
            await _coordinatorStart.Run(_httpRequest, _caseUrn, _caseId, _mockDurableOrchestrationClient.Object);

            _mockOrchestrationProvider.Verify(
                client => client.RefreshCaseAsync(
                    _mockDurableOrchestrationClient.Object,
                    _correlationId,
                    _caseId.ToString(),
                    It.Is<CaseOrchestrationPayload>(p => p.CmsCaseId == _caseId),
                    _httpRequest));
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
                _mockDurableOrchestrationClient.Setup(client => client.GetStatusAsync(_caseId.ToString(), false, false, true))
                    .ReturnsAsync(new DurableOrchestrationStatus { RuntimeStatus = runtimeStatus });

                await _coordinatorStart.Run(_httpRequest, _caseUrn, _caseId, _mockDurableOrchestrationClient.Object);
            }

            _mockDurableOrchestrationClient.Verify(
                client => client.StartNewAsync(
                    nameof(RefreshCaseOrchestrator),
                    _caseId.ToString(),
                    It.Is<CaseOrchestrationPayload>(p => p.CmsCaseId == _caseId)),
                Times.Never);
        }

        [Fact]
        public async Task Run_ReturnsExpectedHttpResponseMessage()
        {
            // Act
            var httpResponseMessage = await _coordinatorStart.Run(_httpRequest, _caseUrn, _caseId, _mockDurableOrchestrationClient.Object);

            // Assert
            httpResponseMessage.Should().Be(_actionResult);
        }
    }
}