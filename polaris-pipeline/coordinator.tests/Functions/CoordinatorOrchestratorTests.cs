using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Common.Constants;
using Common.Domain.DocumentExtraction;
using Common.Domain.Extensions;
using Common.Domain.Responses;
using coordinator.Domain;
using coordinator.Domain.Exceptions;
using coordinator.Domain.Tracker;
using coordinator.Functions;
using coordinator.Functions.ActivityFunctions;
using coordinator.Functions.SubOrchestrators;
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
        private readonly CoordinatorOrchestrationPayload _payload;
        private readonly string _cmsAuthValues;
        private readonly CmsCaseDocument[] _caseDocuments;
        private readonly string _transactionId;
        private readonly List<TrackerDocument> _trackerDocuments;

        private readonly Mock<IDurableOrchestrationContext> _mockDurableOrchestrationContext;
        private readonly Mock<ITracker> _mockTracker;

        private readonly CoordinatorOrchestrator _coordinatorOrchestrator;

        public CoordinatorOrchestratorTests()
        {
            var fixture = new Fixture();
            _cmsAuthValues = fixture.Create<string>();
            fixture.Create<Guid>();
            var durableRequest = new DurableHttpRequest(HttpMethod.Post, new Uri("https://www.google.co.uk"));
            _payload = fixture.Build<CoordinatorOrchestrationPayload>()
                        .With(p => p.ForceRefresh, false)
                        .With(p => p.CmsAuthValues, _cmsAuthValues)
                        .Create();
            _caseDocuments = fixture.Create<CmsCaseDocument[]>();

            _transactionId = fixture.Create<string>();
            _trackerDocuments = fixture.Create<List<TrackerDocument>>();
            var evaluateDocumentsResponse = fixture.CreateMany<EvaluateDocumentResponse>().ToList();

            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<CoordinatorOrchestrator>>();
            _mockDurableOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockTracker = new Mock<ITracker>();

            mockConfiguration.Setup(config => config[ConfigKeys.CoordinatorKeys.CoordinatorOrchestratorTimeoutSecs]).Returns("300");

            _mockTracker.Setup(tracker => tracker.GetDocuments()).ReturnsAsync(_trackerDocuments);
            _mockTracker.Setup(tracker => tracker.IsStale(false)).ReturnsAsync(true); //default, marked as Stale to perform a new run

            _mockDurableOrchestrationContext.Setup(context => context.GetInput<CoordinatorOrchestrationPayload>())
                .Returns(_payload);
            _mockDurableOrchestrationContext.Setup(context => context.InstanceId)
                .Returns(_transactionId);
            _mockDurableOrchestrationContext.Setup(context => context.CreateEntityProxy<ITracker>(
                    It.Is<EntityId>(e => e.EntityName == nameof(Tracker).ToLower() && e.EntityKey == _payload.CmsCaseId.ToString())))
                .Returns(_mockTracker.Object);
            _mockDurableOrchestrationContext.Setup(context => context.CallActivityAsync<CmsCaseDocument[]>(nameof(GetCaseDocuments),
                    It.Is<GetCaseDocumentsActivityPayload>(p => p.CmsCaseId == _payload.CmsCaseId
                    && p.CmsAuthValues == _payload.CmsAuthValues && p.CorrelationId == _payload.CorrelationId)))
                .ReturnsAsync(_caseDocuments);

            var durableResponse = new DurableHttpResponse(HttpStatusCode.OK, content: evaluateDocumentsResponse.ToJson());
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(durableRequest)).ReturnsAsync(durableResponse);

            _coordinatorOrchestrator = new CoordinatorOrchestrator(mockLogger.Object, mockConfiguration.Object);
        }

        [Fact]
        public async Task Run_ThrowsWhenPayloadIsNull()
        {
            _mockDurableOrchestrationContext.Setup(context => context.GetInput<CoordinatorOrchestrationPayload>())
                .Returns(default(CoordinatorOrchestrationPayload));

            await Assert.ThrowsAsync<ArgumentException>(() => _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_ReturnsDocumentsWhenTrackerAlreadyProcessedAndForceRefreshIsFalse()
        {
            _mockTracker.Setup(tracker => tracker.IsAlreadyProcessed()).ReturnsAsync(true);

            var documents = await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            documents.Should().BeEquivalentTo(_trackerDocuments);
        }

        [Fact]
        public async Task Run_DoesNotInitialiseWhenTrackerAlreadyProcessedAndForceRefreshIsFalse()
        {
            _mockTracker.Setup(tracker => tracker.IsAlreadyProcessed()).ReturnsAsync(true);
            _mockTracker.Setup(tracker => tracker.IsStale(false)).ReturnsAsync(false);

            await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockTracker.Verify(tracker => tracker.Initialise(_transactionId), Times.Never);
        }

        [Fact]
        public async Task Run_Tracker_DoesNotInitialiseTheTrackerWhenIsAlreadyProcessed()
        {
            _mockTracker.Setup(tracker => tracker.IsAlreadyProcessed()).ReturnsAsync(true);
            _mockTracker.Setup(tracker => tracker.IsStale(false)).ReturnsAsync(false);

            await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockTracker.Verify(tracker => tracker.Initialise(_transactionId), Times.Never);
        }

        [Fact]
        public async Task Run_Tracker_InitialisesTheTrackerWhenIsAlreadyProcessed_ButForceRefresh_IsTrue()
        {
            _mockTracker.Setup(tracker => tracker.IsAlreadyProcessed()).ReturnsAsync(true);
            _mockTracker.Setup(tracker => tracker.IsStale(true)).ReturnsAsync(true); //default, marked as Stale to perform a new run

            await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockTracker.Verify(tracker => tracker.Initialise(_transactionId), Times.Once);
        }

        [Fact]
        public async Task Run_Tracker_InitialisesTheTrackerWhenIsAlreadyProcessed_ForceRefreshIsFalse_ButIsStaleReturnsTrue()
        {
            _mockTracker.Setup(tracker => tracker.IsAlreadyProcessed()).ReturnsAsync(true);
            _mockTracker.Setup(tracker => tracker.IsStale(false)).ReturnsAsync(true); //default, marked as Stale to perform a new run

            await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockTracker.Verify(tracker => tracker.Initialise(_transactionId), Times.Once);
        }

        [Fact]
        public async Task Run_Tracker_Initialises()
        {
            await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockTracker.Verify(tracker => tracker.Initialise(_transactionId));
        }

        [Fact]
        public async Task Run_Tracker_RegistersDocumentsNotFoundInDDEIWhenCaseDocumentsIsEmpty()
        {
            _mockDurableOrchestrationContext.Setup(context => context.CallActivityAsync<CmsCaseDocument[]>(nameof(GetCaseDocuments), It.Is<GetCaseDocumentsActivityPayload>(p => p.CmsCaseId == _payload.CmsCaseId && p.CmsAuthValues == _cmsAuthValues)))
                .ReturnsAsync(new CmsCaseDocument[] { });

            await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockTracker.Verify(tracker => tracker.RegisterNoDocumentsFoundInDDEI());
        }

        [Fact]
        public async Task Run_ReturnsEmptyListOfDocumentsWhenCaseDocumentsIsEmpty()
        {
            _mockDurableOrchestrationContext.Setup(context => context.CallActivityAsync<CmsCaseDocument[]>(nameof(GetCaseDocuments), It.Is<GetCaseDocumentsActivityPayload>(p => p.CmsCaseId == _payload.CmsCaseId && p.CmsAuthValues == _cmsAuthValues)))
                .ReturnsAsync(new CmsCaseDocument[] { });

            var documents = await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            documents.Should().BeEmpty();
        }

        [Fact]
        public async Task Run_CallsSubOrchestratorForEachDocumentId()
        {
            await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            foreach (var document in _trackerDocuments)
            {
                _mockDurableOrchestrationContext.Verify
                (
                    context => context.CallSubOrchestratorAsync
                    (
                        nameof(CaseDocumentOrchestrator),
                        It.Is<CaseDocumentOrchestrationPayload>(p => p.CmsCaseId == _payload.CmsCaseId && p.CmsDocumentId == document.CmsDocumentId)
                    )
                );
            }
        }

        [Fact]
        public async Task Run_DoesNotThrowWhenSubOrchestratorCallFails()
        {
            _mockDurableOrchestrationContext.Setup(
                context => context.CallSubOrchestratorAsync(nameof(CaseDocumentOrchestrator), It.IsAny<CaseDocumentOrchestrationPayload>()))
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

            await Assert.ThrowsAsync<CoordinatorOrchestrationException>(() => _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_Tracker_RegistersCompleted()
        {
            await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            _mockTracker.Verify(tracker => tracker.RegisterCompleted());
        }

        [Fact]
        public async Task Run_ReturnsDocuments()
        {
            var documents = await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            documents.Should().BeEquivalentTo(_trackerDocuments);
        }

        [Fact]
        public async Task Run_ThrowsExceptionWhenExceptionOccurs()
        {
            _mockDurableOrchestrationContext.Setup(context => context.CallActivityAsync<CmsCaseDocument[]>(nameof(GetCaseDocuments), It.Is<GetCaseDocumentsActivityPayload>(p => p.CmsCaseId == _payload.CmsCaseId && p.CmsAuthValues == _cmsAuthValues)))
                .ThrowsAsync(new Exception("Test Exception"));

            await Assert.ThrowsAsync<Exception>(() => _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_Tracker_RegistersFailedWhenExceptionOccurs()
        {
            _mockDurableOrchestrationContext.Setup(context => context.CallActivityAsync<CmsCaseDocument[]>(nameof(GetCaseDocuments), It.Is<GetCaseDocumentsActivityPayload>(p => p.CmsCaseId == _payload.CmsCaseId && p.CmsAuthValues == _cmsAuthValues)))
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
