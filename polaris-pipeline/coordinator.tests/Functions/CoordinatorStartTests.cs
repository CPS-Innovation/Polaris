﻿using AutoFixture;
using Common.Configuration;
using Common.Exceptions;
using Common.Services.BlobStorage;
using coordinator.Durable.Entity;
using coordinator.Durable.Orchestration;
using coordinator.Durable.Payloads;
using coordinator.Durable.Providers;
using coordinator.Functions;
using coordinator.Services.ClearDownService;
using Ddei;
using Ddei.Domain.CaseData.Args.Core;
using Ddei.Factories;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

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
        private readonly Mock<IDurableOrchestrationClient> _mockDurableOrchestrationClient;
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
            _instanceId = CaseDurableEntity.GetKey(_caseId);

            _httpRequest = new DefaultHttpContext().Request;
            _httpRequest.Method = "POST";
            _httpRequestHeaders = _httpRequest.Headers;

            _mockDurableOrchestrationClient = new Mock<IDurableOrchestrationClient>();
            var mockLogger = new Mock<ILogger<RefreshCase>>();

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(x => x[StorageKeys.BlobServiceContainerNameDocuments]).Returns("Documents");

            var mockBlobStorageClient = new Mock<IPolarisBlobStorageService>();
            var mockStorageDelegate = new Mock<Func<string, IPolarisBlobStorageService>>();
            mockStorageDelegate.Setup(s => s("Documents")).Returns(mockBlobStorageClient.Object);

            _mockOrchestrationProvider = new Mock<IOrchestrationProvider>();
            _mockCleardownService = new Mock<IClearDownService>();

            _httpRequestHeaders.Add("Correlation-Id", _correlationId.ToString());
            _httpRequestHeaders.Add("cms-auth-values", cmsAuthValues);

            mockBlobStorageClient.Setup(s => s.DeleteBlobsByPrefixAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            _mockDurableOrchestrationClient.Setup(client => client.GetStatusAsync(_instanceId, false, false, true))
               .ReturnsAsync(default(DurableOrchestrationStatus));

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
            _httpRequestHeaders.Clear();

            var result = await _coordinatorStart.Run(_httpRequest, _caseUrn, _caseId, _mockDurableOrchestrationClient.Object);

            (result as StatusCodeResult).StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Run_ReturnsInternalServerErrorWhenUnhandledErrorOccurs()
        {
            _mockDurableOrchestrationClient.Setup(client => client.StartNewAsync(nameof(RefreshCaseOrchestrator), _instanceId, It.IsAny<CasePayload>()))
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
                    _caseId,
                    It.IsAny<CasePayload>(),
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
                    _caseId,
                    It.Is<CasePayload>(p => p.CaseId == _caseId),
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
                    It.Is<CasePayload>(p => p.CaseId == _caseId)),
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
            _mockDdeiClient.Setup(client => client.VerifyCmsAuthAsync(_mockVerifyArg)).ThrowsAsync(new DdeiClientException(HttpStatusCode.Unauthorized, null));
            // Act
            var httpResponseMessage = await _coordinatorStart.Run(_httpRequest, _caseUrn, _caseId, _mockDurableOrchestrationClient.Object);

            // Assert
            httpResponseMessage.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be((int)HttpStatusCode.Unauthorized);
        }
    }
}