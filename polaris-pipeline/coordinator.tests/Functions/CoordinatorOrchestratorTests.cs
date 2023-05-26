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
using System.Linq.Expressions;
using coordinator.Functions.DurableEntity.Entity.Contract;

namespace coordinator.tests.Functions
{
    public class CoordinatorOrchestratorTests
    {
        private readonly CaseOrchestrationPayload _payload;
        private readonly string _cmsAuthValues;
        private readonly long _cmsCaseId;
        private readonly string _cmsCaseUrn;
        private readonly Guid _polarisDocumentId;
        private readonly Guid _correlationId;
        private readonly (DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) _caseDocuments;
        private readonly string _transactionId;
        private readonly List<TrackerCmsDocumentDto> _trackerCmsDocuments;
        private readonly TrackerDeltasDto _deltaDocuments;

        private readonly Mock<IDurableOrchestrationContext> _mockDurableOrchestrationContext;
        private readonly Mock<ICaseEntity> _mockTracker;

        private readonly RefreshCaseOrchestrator _coordinatorOrchestrator;

        public CoordinatorOrchestratorTests()
        {
            var fixture = new Fixture();
            _cmsAuthValues = fixture.Create<string>();
            _cmsCaseUrn = fixture.Create<string>();
            _cmsCaseId = fixture.Create<long>();
            _correlationId = fixture.Create<Guid>();    
            _polarisDocumentId = fixture.Create<Guid>();
            fixture.Create<Guid>();
            var durableRequest = new DurableHttpRequest(HttpMethod.Post, new Uri("https://www.google.co.uk"));
            _payload = fixture.Build<CaseOrchestrationPayload>()
                        .With(p => p.CmsAuthValues, _cmsAuthValues)
                        .With(p => p.CmsCaseId, _cmsCaseId)
                        .With(p => p.CmsCaseUrn, _cmsCaseUrn)
                        .With(p => p.CorrelationId, _correlationId)
                        .With(p => p.PolarisDocumentId, _polarisDocumentId)
                        .Create();
            _caseDocuments = fixture.Create<(DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>();

            _transactionId = fixture.Create<string>();
            _trackerCmsDocuments = fixture.CreateMany<TrackerCmsDocumentDto>(11).ToList();
            _deltaDocuments = new TrackerDeltasDto
            {
                CreatedCmsDocuments = _trackerCmsDocuments.Where(d => d.Status == TrackerDocumentStatus.New).ToList(),
                UpdatedCmsDocuments = fixture.Create<TrackerCmsDocumentDto[]>().ToList(),
                DeletedCmsDocuments = fixture.Create<TrackerCmsDocumentDto[]>().ToList(),
                CreatedPcdRequests = new List<TrackerPcdRequestDto> { },
                UpdatedPcdRequests = new List<TrackerPcdRequestDto> { },
                DeletedPcdRequests = new List<TrackerPcdRequestDto> { },
                CreatedDefendantsAndCharges = fixture.Create<TrackerDefendantsAndChargesDto>(),
                UpdatedDefendantsAndCharges = fixture.Create<TrackerDefendantsAndChargesDto>(),
                IsDeletedDefendantsAndCharges = false
            };
            var evaluateDocumentsResponse = fixture.CreateMany<EvaluateDocumentResponse>().ToList();

            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<RefreshCaseOrchestrator>>();
            _mockDurableOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockTracker = new Mock<ICaseEntity>();

            mockConfiguration.Setup(config => config[ConfigKeys.CoordinatorKeys.CoordinatorOrchestratorTimeoutSecs]).Returns("300");

            //_mockTracker
            //    .Setup(tracker => tracker.GetDocuments())
            //    .ReturnsAsync(_trackerCmsDocuments);

            //var casDocumentChangesArg = (It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<DocumentDto[]>(), It.IsAny<PcdRequestDto[]>(), It.IsAny<DefendantsAndChargesListDto>(), It.IsAny<Guid>());
            _mockTracker
                .Setup(tracker => tracker.GetCaseDocumentChanges(((DateTime CurrentUtcDateTime, string CmsCaseUrn, long CmsCaseId, DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges, Guid CorrelationId))It.IsAny<object>()))
                .ReturnsAsync(_deltaDocuments);

            _mockTracker
                .Setup(t => t.AllDocumentsFailed())
                .ReturnsAsync(false);

            _mockDurableOrchestrationContext
                .Setup(context => context.GetInput<CaseOrchestrationPayload>())
                .Returns(_payload);

            _mockDurableOrchestrationContext
                .Setup(context => context.InstanceId)
                .Returns(_transactionId);

            _mockDurableOrchestrationContext
                .Setup(context => context.CreateEntityProxy<ICaseEntity>(It.Is<EntityId>(e => e.EntityName == nameof(CaseEntity).ToLower() && e.EntityKey == _payload.CmsCaseId.ToString())))
                .Returns(_mockTracker.Object);

            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<(DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>(nameof(GetCaseDocuments), It.IsAny<GetCaseDocumentsActivityPayload>()))
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

            var arg = (It.IsAny<DateTime>(), _transactionId);
            _mockTracker.Verify(tracker => tracker.Reset(arg));
        }

        [Fact]
        public async Task Run_DoesntCallDocumentTasksWhenCaseDocumentsIsEmpty()
        {
            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<(DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>(nameof(GetCaseDocuments), It.IsAny<GetCaseDocumentsActivityPayload>()))
                .ReturnsAsync((new DocumentDto[0], new PcdRequestDto[0], new DefendantsAndChargesListDto()));

            _mockTracker
                .Setup(tracker => tracker.GetCaseDocumentChanges(It.IsAny<(DateTime CurrentUtcDateTime, string CmsCaseUrn, long CmsCaseId, DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges, Guid CorrelationId)>()))
                .ReturnsAsync(new TrackerDeltasDto
                {
                    CreatedCmsDocuments = new List<TrackerCmsDocumentDto>(),
                    UpdatedCmsDocuments = new List<TrackerCmsDocumentDto>(),
                    DeletedCmsDocuments = new List<TrackerCmsDocumentDto>(),
                    CreatedPcdRequests = new List<TrackerPcdRequestDto>(),
                    UpdatedPcdRequests = new List<TrackerPcdRequestDto>(),
                    DeletedPcdRequests = new List<TrackerPcdRequestDto>(),
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

            foreach (var document in _trackerCmsDocuments.Where(t => t.Status == TrackerDocumentStatus.New))
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
            _mockTracker.Setup(t => t.AllDocumentsFailed()).ReturnsAsync(true);

            await Assert.ThrowsAsync<CaseOrchestrationException>(() => _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_Tracker_RegistersCompleted()
        {
            await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            var arg = (It.IsAny<DateTime>(), true);
            _mockTracker.Verify(tracker => tracker.RegisterCompleted(arg));
        }

        [Fact]
        public async Task Run_ThrowsExceptionWhenExceptionOccurs()
        {
            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<(DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>(nameof(GetCaseDocuments), It.IsAny<GetCaseDocumentsActivityPayload>()))
                .ThrowsAsync(new Exception("Test Exception"));

            await Assert.ThrowsAsync<Exception>(() => _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_Tracker_RegistersFailedWhenExceptionOccurs()
        {
            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<(DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>(nameof(GetCaseDocuments), It.IsAny<GetCaseDocumentsActivityPayload>()))
                .ThrowsAsync(new Exception("Test Exception"));

            try
            {
                await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);
                Assert.False(true);
            }
            catch
            {
                var arg = (It.IsAny<DateTime>(), false);
                _mockTracker.Verify(tracker => tracker.RegisterCompleted(arg));
            }
        }
    }
}
