using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using Common.Services.BlobStorage;
using coordinator.Durable.Payloads;
using coordinator.Functions;
using coordinator.Durable.Orchestration;
using coordinator.Durable.Providers;
using coordinator.Services.ClearDownService;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ddei.Factories;
using Ddei;
using Ddei.Domain.CaseData.Args.Core;
using Common.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask;
using Common.Exceptions;
using Common.Constants;
using DdeiClient.Clients.Interfaces;

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
        private readonly DdeiBaseArgDto _mockVerifyArg;
        private readonly Mock<DurableTaskClient> _mockDurableOrchestrationClient;
        private readonly Mock<IOrchestrationProvider> _mockOrchestrationProvider;
        private readonly Mock<IClearDownService> _mockCleardownService;
        private readonly Mock<IDdeiArgFactory> _mockDdeiArgFactory;
        private readonly Mock<IDdeiClient> _mockDdeiClient;
        private readonly RefreshCase _coordinatorStart;

        public CoordinatorStartTests()
        {
            var fixture = new Fixture();
            _caseUrn = fixture.Create<string>();
            _caseId = fixture.Create<int>();

            var cmsAuthValues = fixture.Create<string>();
            _correlationId = fixture.Create<Guid>();
            _instanceId = OrchestrationProvider.GetKey(_caseId);

            _httpRequest = new DefaultHttpContext().Request;
            _httpRequest.Method = "POST";
            _httpRequestHeaders = _httpRequest.Headers;

            _mockDurableOrchestrationClient = new Mock<DurableTaskClient>("name");
            var mockLogger = new Mock<ILogger<RefreshCase>>();

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(x => x[StorageKeys.BlobServiceContainerNameDocuments]).Returns("Documents");

            var mockBlobStorageClient = new Mock<IPolarisBlobStorageService>();
            var mockStorageDelegate = new Mock<Func<string, IPolarisBlobStorageService>>();
            mockStorageDelegate.Setup(s => s("Documents")).Returns(mockBlobStorageClient.Object);

            _mockOrchestrationProvider = new Mock<IOrchestrationProvider>();
            _mockCleardownService = new Mock<IClearDownService>();

            _httpRequestHeaders.Append(HttpHeaderKeys.CorrelationId, _correlationId.ToString());
            _httpRequestHeaders.Append(HttpHeaderKeys.CmsAuthValues, cmsAuthValues);

            mockBlobStorageClient.Setup(s => s.DeleteBlobsByPrefixAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            _mockDurableOrchestrationClient.Setup(client => client.GetInstanceAsync(_instanceId, default))
               .ReturnsAsync(new OrchestrationMetadata("orchestrationName", _instanceId) { RuntimeStatus = default });

            _mockOrchestrationProvider.Setup(s => s.RefreshCaseAsync(_mockDurableOrchestrationClient.Object,
                    It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CasePayload>(), _httpRequest))
                .ReturnsAsync(true);
            _mockOrchestrationProvider.Setup(s => s.DeleteCaseOrchestrationAsync(_mockDurableOrchestrationClient.Object,
                    It.IsAny<int>()));

            _mockCleardownService.Setup(s => s.DeleteCaseAsync(_mockDurableOrchestrationClient.Object,
                    It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Guid>()));

            _mockVerifyArg = fixture.Create<DdeiBaseArgDto>();
            _mockDdeiArgFactory = new Mock<IDdeiArgFactory>();
            _mockDdeiArgFactory.Setup(factory => factory.CreateCmsCaseDataArgDto(cmsAuthValues, _correlationId))
                .Returns(_mockVerifyArg);
            _mockDdeiClient = new Mock<IDdeiClient>();
            _mockDdeiClient.Setup(client => client.VerifyCmsAuthAsync(_mockVerifyArg));

            _coordinatorStart = new RefreshCase(mockLogger.Object, _mockOrchestrationProvider.Object, _mockCleardownService.Object, _mockDdeiArgFactory.Object, _mockDdeiClient.Object);
        }

        [Fact]
        public async Task Run_ReturnsBadRequestWhenCorrelationIdIsMissing()
        {
            // Arrange
            _httpRequestHeaders.Clear();

            // Act
            var exception = await Assert.ThrowsAsync<BadRequestException>(() => _coordinatorStart.Run(_httpRequest, _caseUrn, _caseId, _mockDurableOrchestrationClient.Object));

            // Assert
            exception.Message.Should().Be("Invalid correlationId. A valid GUID is required. (Parameter 'headers')");
        }

        [Fact]
        public async Task Run_ReturnsInternalServerErrorWhenUnhandledErrorOccurs()
        {
            _mockDurableOrchestrationClient.Setup(client => client.ScheduleNewOrchestrationInstanceAsync(nameof(RefreshCaseOrchestrator), It.IsAny<CasePayload>(), It.IsAny<StartOrchestrationOptions>(), default))
                .Throws(new Exception());

            _mockOrchestrationProvider.Setup(s => s.RefreshCaseAsync(_mockDurableOrchestrationClient.Object,
                    It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CasePayload>(), _httpRequest))
                .ReturnsAsync(false);

            var result = await _coordinatorStart.Run(_httpRequest, _caseUrn, _caseId, _mockDurableOrchestrationClient.Object);

            (result as ObjectResult).StatusCode.Should().Be((int)HttpStatusCode.Locked);
        }

        [Fact]
        public async Task Run_StartsOrchestratorWhenOrchestrationStatusIsNull()
        {
            // Arrange
            // Act
            await _coordinatorStart.Run(_httpRequest, _caseUrn, _caseId, _mockDurableOrchestrationClient.Object);

            // Assert
            _mockOrchestrationProvider.Verify(
                client => client.RefreshCaseAsync(
                    _mockDurableOrchestrationClient.Object,
                    _correlationId,
                    _caseId,
                    It.IsAny<CasePayload>(),
                    _httpRequest));
        }

        [Theory]
        [InlineData(OrchestrationRuntimeStatus.Completed)]
        [InlineData(OrchestrationRuntimeStatus.Terminated)]
        [InlineData(OrchestrationRuntimeStatus.Failed)]
        public async Task Run_StartsOrchestratorWhenOrchestrationHasConcluded(OrchestrationRuntimeStatus runtimeStatus)
        {
            _mockDurableOrchestrationClient.Setup(client => client.GetInstanceAsync(_instanceId, default))
               .ReturnsAsync(new OrchestrationMetadata("orchestrationName", _instanceId) { RuntimeStatus = runtimeStatus });

            await _coordinatorStart.Run(_httpRequest, _caseUrn, _caseId, _mockDurableOrchestrationClient.Object);

            _mockOrchestrationProvider.Verify(
                client => client.RefreshCaseAsync(
                    _mockDurableOrchestrationClient.Object,
                    _correlationId,
                    _caseId,
                    It.Is<CasePayload>(p => p.CaseId == _caseId),
                    _httpRequest));
        }

        [Fact]
        public async Task Run_DoesNotStartOrchestratorWhenOrchestrationHasNotConcluded()
        {
            var notStartingRuntimeStatuses = Enum.GetValues(typeof(OrchestrationRuntimeStatus))
                .Cast<OrchestrationRuntimeStatus>()
                .Except([
                    OrchestrationRuntimeStatus.Completed,
                    OrchestrationRuntimeStatus.Terminated,
                    OrchestrationRuntimeStatus.Failed,
                ]);

            foreach (var runtimeStatus in notStartingRuntimeStatuses)
            {
                _mockDurableOrchestrationClient.Setup(client => client.GetInstanceAsync(_instanceId, default))
                   .ReturnsAsync(new OrchestrationMetadata("orchestrationName", _instanceId) { RuntimeStatus = runtimeStatus });

                await _coordinatorStart.Run(_httpRequest, _caseUrn, _caseId, _mockDurableOrchestrationClient.Object);
            }

            _mockDurableOrchestrationClient.Verify(
                client => client.ScheduleNewOrchestrationInstanceAsync(
                    nameof(RefreshCaseOrchestrator),
                    It.Is<CasePayload>(p => p.CaseId == _caseId),
                    It.IsAny<StartOrchestrationOptions>(),
                    default),
                Times.Never);
        }

        [Fact]
        public async Task Run_ReturnsExpectedHttpResponseMessage()
        {
            // Act
            var httpResponseMessage = await _coordinatorStart.Run(_httpRequest, _caseUrn, _caseId, _mockDurableOrchestrationClient.Object);

            // Assert
            httpResponseMessage.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Fact]
        public async Task Run_Returns401HttpResponseMessage_WhenCmsAuthIsNotValid()
        {
            // Arrange
            _mockDdeiClient.Setup(client => client.VerifyCmsAuthAsync(_mockVerifyArg)).ThrowsAsync(new DdeiClientException(HttpStatusCode.Unauthorized, null));

            // Act
            var exception = await Assert.ThrowsAsync<DdeiClientException>(() => _coordinatorStart.Run(_httpRequest, _caseUrn, _caseId, _mockDurableOrchestrationClient.Object));

            // Assert
            exception.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}