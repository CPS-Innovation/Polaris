using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Common.Constants;
using Common.Domain.Case;
using Common.Domain.Extensions;
using Common.Domain.Responses;
using coordinator.Domain;
using coordinator.Domain.Exceptions;
using coordinator.Domain.Tracker;
using coordinator.Functions.ActivityFunctions.Case;
using coordinator.Functions.DurableEntity.Entity;
using coordinator.Functions.Orchestation.Functions.Case;
using coordinator.Functions.Orchestation.Functions.Document;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Functions
{
    public class CoordinatorOrchestratorTests
    {
        private readonly CaseOrchestrationPayload _payload;
        private readonly string _cmsAuthValues;
        private readonly TransitionDocument[] _caseDocuments;
        private readonly string _transactionId;
        private readonly List<TrackerDocument> _trackerDocuments;
        private readonly TrackerDocumentListDeltas _newOrChangedDocuments;

        private readonly Mock<IDurableOrchestrationContext> _mockDurableOrchestrationContext;
        private readonly Mock<ITrackerEntity> _mockTracker;

        private readonly RefreshCaseOrchestrator _coordinatorOrchestrator;

        public CoordinatorOrchestratorTests()
        {
            var fixture = new Fixture();
            _cmsAuthValues = fixture.Create<string>();
            fixture.Create<Guid>();
            var durableRequest = new DurableHttpRequest(HttpMethod.Post, new Uri("https://www.google.co.uk"));
            _payload = fixture.Build<CaseOrchestrationPayload>()
                        .With(p => p.CmsAuthValues, _cmsAuthValues)
                        .Create();
            _caseDocuments = fixture.Create<TransitionDocument[]>();

            _transactionId = fixture.Create<string>();
            _trackerDocuments = fixture.CreateMany<TrackerDocument>(11).ToList();
            _newOrChangedDocuments = new TrackerDocumentListDeltas { CreatedOrUpdated = _trackerDocuments.Where(d => d.Status == DocumentStatus.New).ToList(), Deleted = fixture.Create<DocumentVersion[]>().ToList() };
            var evaluateDocumentsResponse = fixture.CreateMany<EvaluateDocumentResponse>().ToList();

            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<RefreshCaseOrchestrator>>();
            _mockDurableOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockTracker = new Mock<ITrackerEntity>();

            mockConfiguration.Setup(config => config[ConfigKeys.CoordinatorKeys.CoordinatorOrchestratorTimeoutSecs]).Returns("300");

            _mockTracker.Setup(tracker => tracker.GetDocuments()).ReturnsAsync(_trackerDocuments);
            _mockTracker.Setup(tracker => tracker.SynchroniseDocuments(It.IsAny<SynchroniseDocumentsArg>())).ReturnsAsync(_newOrChangedDocuments);

            _mockDurableOrchestrationContext.Setup(context => context.GetInput<CaseOrchestrationPayload>())
                .Returns(_payload);
            _mockDurableOrchestrationContext.Setup(context => context.InstanceId)
                .Returns(_transactionId);
            _mockDurableOrchestrationContext.Setup(context => context.CreateEntityProxy<ITrackerEntity>(
                    It.Is<EntityId>(e => e.EntityName == nameof(TrackerEntity).ToLower() && e.EntityKey == _payload.CmsCaseId.ToString())))
                .Returns(_mockTracker.Object);
            _mockDurableOrchestrationContext.Setup(context => context.CallActivityAsync<TransitionDocument[]>(nameof(GetCaseDocuments),
                    It.Is<GetCaseDocumentsActivityPayload>(p => p.CmsCaseId == _payload.CmsCaseId
                    && p.CmsAuthValues == _payload.CmsAuthValues && p.CorrelationId == _payload.CorrelationId)))
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

            _mockTracker.Verify(tracker => tracker.Reset(_transactionId));
        }

        [Fact]
        public async Task Run_ReturnsEmptyListOfDocumentsWhenCaseDocumentsIsEmpty()
        {
            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<TransitionDocument[]>(nameof(GetCaseDocuments), It.Is<GetCaseDocumentsActivityPayload>(p => p.CmsCaseId == _payload.CmsCaseId && p.CmsAuthValues == _cmsAuthValues)))
                .ReturnsAsync(new TransitionDocument[] { });

            _mockTracker
                .Setup(tracker => tracker.SynchroniseDocuments(It.IsAny<SynchroniseDocumentsArg>()))
                .ReturnsAsync(new TrackerDocumentListDeltas { CreatedOrUpdated = new List<TrackerDocument>(), Deleted = new List<DocumentVersion>() });

            var documents = await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            documents.Should().BeEmpty();
        }

        [Fact]
        public async Task Run_CallsSubOrchestratorForEachNewOrChangedDocument()
        {
            await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            foreach (var document in _trackerDocuments.Where(t => t.Status == DocumentStatus.New))
            {
                _mockDurableOrchestrationContext.Verify
                (
                    context => context.CallSubOrchestratorAsync
                    (
                        nameof(RefreshDocumentOrchestrator),
                        It.Is<CaseDocumentOrchestrationPayload>(p => p.CmsCaseId == _payload.CmsCaseId && p.CmsDocumentId == document.CmsDocumentId)
                    )
                );
            }
        }

        [Fact]
        public async Task Run_DoesNotThrowWhenSubOrchestratorCallFails()
        {
            _mockDurableOrchestrationContext.Setup(
                context => context.CallSubOrchestratorAsync(nameof(RefreshDocumentOrchestrator), It.IsAny<CaseDocumentOrchestrationPayload>()))
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

            _mockTracker.Verify(tracker => tracker.RegisterCompleted());
        }

        [Fact]
        public async Task Run_ReturnsNewOrChangedDocuments()
        {
            var documents = await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            documents.Should().BeEquivalentTo(_newOrChangedDocuments.CreatedOrUpdated);
        }

        [Fact]
        public async Task Run_ThrowsExceptionWhenExceptionOccurs()
        {
            _mockDurableOrchestrationContext.Setup(context => context.CallActivityAsync<TransitionDocument[]>(nameof(GetCaseDocuments), It.Is<GetCaseDocumentsActivityPayload>(p => p.CmsCaseId == _payload.CmsCaseId && p.CmsAuthValues == _cmsAuthValues)))
                .ThrowsAsync(new Exception("Test Exception"));

            await Assert.ThrowsAsync<Exception>(() => _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_Tracker_RegistersFailedWhenExceptionOccurs()
        {
            _mockDurableOrchestrationContext.Setup(context => context.CallActivityAsync<TransitionDocument[]>(nameof(GetCaseDocuments), It.Is<GetCaseDocumentsActivityPayload>(p => p.CmsCaseId == _payload.CmsCaseId && p.CmsAuthValues == _cmsAuthValues)))
                .ThrowsAsync(new Exception("Test Exception"));

            try
            {
                await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);
                Assert.False(true);
            }
            catch
            {
                _mockTracker.Verify(tracker => tracker.RegisterFailed());
            }
        }
    }
}
