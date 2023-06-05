using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Common.Constants;
using Common.Domain.Extensions;
using Common.Dto.Case.PreCharge;
using Common.Dto.Case;
using Common.Dto.Document;
using Common.Dto.Response;
using Common.Dto.Tracker;
using coordinator.Domain;
using coordinator.Domain.Exceptions;
using coordinator.Functions.ActivityFunctions.Case;
using coordinator.Functions.DurableEntity.Entity;
using coordinator.Functions.Orchestration.Functions.Case;
using coordinator.Functions.Orchestration.Functions.Document;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Common.ValueObjects;
using coordinator.Functions.DurableEntity.Entity.Contract;
using Common.Domain.Entity;

namespace coordinator.tests.Functions
{
    public class CoordinatorOrchestratorTests
    {
        private readonly CaseOrchestrationPayload _payload;
        private readonly string _cmsAuthValues;
        private readonly long _cmsCaseId;
        private readonly string _cmsCaseUrn;
        private readonly PolarisDocumentId _polarisDocumentId;
        private readonly Guid _correlationId;
        private readonly (CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) _caseDocuments;
        private readonly string _transactionId;
        private readonly List<CmsDocumentEntity> _trackerCmsDocuments;
        private readonly CaseDeltasEntity _deltaDocuments;

        private readonly Mock<IDurableOrchestrationContext> _mockDurableOrchestrationContext;
        private readonly Mock<ICaseDurableEntity> _mockCaseEntity;
        private readonly Mock<ICaseRefreshLogsDurableEntity> _mockCaseRefreshLogsEntity;

        private readonly RefreshCaseOrchestrator _coordinatorOrchestrator;

        public CoordinatorOrchestratorTests()
        {
            var fixture = new Fixture();
            _cmsAuthValues = fixture.Create<string>();
            _cmsCaseUrn = fixture.Create<string>();
            _cmsCaseId = fixture.Create<long>();
            _correlationId = fixture.Create<Guid>();    
            _polarisDocumentId = fixture.Create<PolarisDocumentId>();
            fixture.Create<Guid>();
            var durableRequest = new DurableHttpRequest(HttpMethod.Post, new Uri("https://www.google.co.uk"));
            _payload = fixture.Build<CaseOrchestrationPayload>()
                        .With(p => p.CmsAuthValues, _cmsAuthValues)
                        .With(p => p.CmsCaseId, _cmsCaseId)
                        .With(p => p.CmsCaseUrn, _cmsCaseUrn)
                        .With(p => p.CorrelationId, _correlationId)
                        .With(p => p.PolarisDocumentId, _polarisDocumentId)
                        .Create();
            _caseDocuments = fixture.Create<(CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>();

            _transactionId = fixture.Create<string>();
            _trackerCmsDocuments = fixture.CreateMany<CmsDocumentEntity>(11).ToList();
            _deltaDocuments = new CaseDeltasEntity
            {
                CreatedCmsDocuments = _trackerCmsDocuments.Where(d => d.Status == DocumentStatus.New).ToList(),
                UpdatedCmsDocuments = fixture.Create<CmsDocumentEntity[]>().ToList(),
                DeletedCmsDocuments = fixture.Create<CmsDocumentEntity[]>().ToList(),
                CreatedPcdRequests = new List<PcdRequestEntity> { },
                UpdatedPcdRequests = new List<PcdRequestEntity> { },
                DeletedPcdRequests = new List<PcdRequestEntity> { },
                CreatedDefendantsAndCharges = fixture.Create<DefendantsAndChargesEntity>(),
                UpdatedDefendantsAndCharges = fixture.Create<DefendantsAndChargesEntity>(),
                IsDeletedDefendantsAndCharges = false
            };
            var evaluateDocumentsResponse = fixture.CreateMany<EvaluateDocumentResponse>().ToList();

            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<RefreshCaseOrchestrator>>();
            _mockDurableOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockCaseEntity = new Mock<ICaseDurableEntity>();
            _mockCaseRefreshLogsEntity = new Mock<ICaseRefreshLogsDurableEntity>();

            mockConfiguration
                .Setup(config => config[ConfigKeys.CoordinatorKeys.CoordinatorOrchestratorTimeoutSecs])
                .Returns("300");

            _mockCaseEntity
                .Setup(tracker => tracker.GetCaseDocumentChanges(((DateTime t, CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges))It.IsAny<object>()))
                .ReturnsAsync(_deltaDocuments);

            _mockCaseEntity
                .Setup(t => t.AllDocumentsFailed())
                .ReturnsAsync(false);

            _mockDurableOrchestrationContext
                .Setup(context => context.GetInput<CaseOrchestrationPayload>())
                .Returns(_payload);

            _mockDurableOrchestrationContext
                .Setup(context => context.InstanceId)
                .Returns(_transactionId);

            _mockDurableOrchestrationContext
                .Setup(context => context.CreateEntityProxy<ICaseDurableEntity>(It.Is<EntityId>(e => e.EntityName == nameof(CaseDurableEntity).ToLower() && e.EntityKey == _payload.CmsCaseId.ToString())))
                .Returns(_mockCaseEntity.Object);

            _mockDurableOrchestrationContext
                .Setup(context => context.CreateEntityProxy<ICaseRefreshLogsDurableEntity>(It.Is<EntityId>(e => e.EntityName == nameof(CaseRefreshLogsDurableEntity).ToLower() && e.EntityKey.StartsWith(_payload.CmsCaseId.ToString()))))
                .Returns(_mockCaseRefreshLogsEntity.Object);

            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<(CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>(nameof(GetCaseDocuments), It.IsAny<GetCaseDocumentsActivityPayload>()))
                .ReturnsAsync(_caseDocuments);

            var durableResponse = new DurableHttpResponse(HttpStatusCode.OK, content: evaluateDocumentsResponse.ToJson());
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(durableRequest)).ReturnsAsync(durableResponse);

            _coordinatorOrchestrator = new RefreshCaseOrchestrator(mockLogger.Object, mockConfiguration.Object);
        }

        [Fact]
        public async Task Run_ThrowsWhenPayloadIsNull()
        {
            _mockDurableOrchestrationContext.Setup(context => context.GetInput<CaseOrchestrationPayload>())
                .Returns(default(CaseOrchestrationPayload));

            await Assert.ThrowsAsync<ArgumentException>(() => _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_Tracker_Initialises()
        {
            await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockCaseEntity.Verify(tracker => tracker.Reset(_transactionId));
        }

        [Fact]
        public async Task Run_DoesntCallDocumentTasksWhenCaseDocumentsIsEmpty()
        {
            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<(CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>(nameof(GetCaseDocuments), It.IsAny<GetCaseDocumentsActivityPayload>()))
                .ReturnsAsync((new CmsDocumentDto[0], new PcdRequestDto[0], new DefendantsAndChargesListDto()));

            _mockCaseEntity
                .Setup(tracker => tracker.GetCaseDocumentChanges(It.IsAny<(DateTime t, CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>()))
                .ReturnsAsync(new CaseDeltasEntity
                {
                    CreatedCmsDocuments = new List<CmsDocumentEntity>(),
                    UpdatedCmsDocuments = new List<CmsDocumentEntity>(),
                    DeletedCmsDocuments = new List<CmsDocumentEntity>(),
                    CreatedPcdRequests = new List<PcdRequestEntity>(),
                    UpdatedPcdRequests = new List<PcdRequestEntity>(),
                    DeletedPcdRequests = new List<PcdRequestEntity>(),
                    CreatedDefendantsAndCharges = null,
                    UpdatedDefendantsAndCharges = null,
                    IsDeletedDefendantsAndCharges = false
                });

            var tracker = await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockDurableOrchestrationContext.Verify(context => context.CallSubOrchestratorAsync(nameof(RefreshDocumentOrchestrator), It.IsAny<object>()), Times.Never());
        }

        [Fact]
        public async Task Run_CallsSubOrchestratorForEachNewOrChangedDocument()
        {
            await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            foreach (var document in _trackerCmsDocuments.Where(t => t.Status == DocumentStatus.New))
            {
                _mockDurableOrchestrationContext.Verify(context => context.CallSubOrchestratorAsync
                (
                    nameof(RefreshDocumentOrchestrator),
                    It.Is<CaseDocumentOrchestrationPayload>
                    (
                        payload =>
                            payload.CmsCaseId == _payload.CmsCaseId &&
                            (
                                (payload.CmsDocumentTracker != null && payload.CmsDocumentTracker.CmsDocumentId == document.CmsDocumentId)) ||
                                (payload.DefendantAndChargesTracker != null && payload.DefendantAndChargesTracker.CmsDocumentId == document.CmsDocumentId)
                            )
                ));
            }
        }

        [Fact]
        public async Task Run_DoesNotThrowWhenSubOrchestratorCallFails()
        {
            _mockDurableOrchestrationContext.Setup(
                context => context.CallSubOrchestratorAsync(nameof(RefreshDocumentOrchestrator), It.IsAny<GetCaseDocumentsActivityPayload>()))
                    .ThrowsAsync(new Exception());

            try
            {
                await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);
            }
            catch (Exception)
            {
                Assert.True(false);
            }
        }

        [Fact]
        public async Task Run_ThrowsCoordinatorOrchestrationExceptionWhenAllDocumentsHaveFailed()
        {
            _mockCaseEntity.Setup(t => t.AllDocumentsFailed()).ReturnsAsync(true);

            await Assert.ThrowsAsync<CaseOrchestrationException>(() => _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_Tracker_RegistersCompleted()
        {
            await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            var arg = (It.IsAny<DateTime>(), true);
            _mockCaseEntity.Verify(tracker => tracker.RegisterCompleted(arg));
        }

        [Fact]
        public async Task Run_ThrowsExceptionWhenExceptionOccurs()
        {
            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<(CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>(nameof(GetCaseDocuments), It.IsAny<GetCaseDocumentsActivityPayload>()))
                .ThrowsAsync(new Exception("Test Exception"));

            await Assert.ThrowsAsync<Exception>(() => _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_Tracker_RegistersFailedWhenExceptionOccurs()
        {
            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<(CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>(nameof(GetCaseDocuments), It.IsAny<GetCaseDocumentsActivityPayload>()))
                .ThrowsAsync(new Exception("Test Exception"));

            try
            {
                await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);
                Assert.False(true);
            }
            catch
            {
                var arg = (It.IsAny<DateTime>(), false);
                _mockCaseEntity.Verify(tracker => tracker.RegisterCompleted(arg));
            }
        }
    }
}
