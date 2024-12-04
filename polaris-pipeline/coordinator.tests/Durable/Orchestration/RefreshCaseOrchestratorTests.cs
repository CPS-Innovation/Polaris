using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Common.Dto.Response.Case;
using Common.Dto.Response.Document;
using Common.Dto.Response;
using Common.Dto.Response.Documents;
using coordinator.Constants;
using coordinator.Durable.Activity;
using coordinator.Durable.Orchestration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using coordinator.Durable.Entity;
using Common.Telemetry;
using coordinator.Validators;
using coordinator.Durable.Payloads;
using coordinator.Durable.Payloads.Domain;
using coordinator.Domain;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;

namespace coordinator.tests.Durable.Orchestration
{
    public class RefreshCaseOrchestratorTests
    {
        private readonly CasePayload _payload;
        private readonly string _cmsAuthValues;
        private readonly long _caseId;
        private readonly string _urn;
        private readonly Guid _correlationId;
        private readonly GetCaseDocumentsResponse _caseDocuments;
        private readonly string _transactionId;
        private readonly List<DocumentDelta> _trackerCmsDocuments;
        private readonly CaseDeltasEntity _deltaDocuments;
        private readonly Mock<TaskOrchestrationContext> _mockTaskOrchestrationContext;
        private readonly Mock<TaskOrchestrationEntityFeature> _mockTaskOrchestrationEntityFeature;
        private readonly Mock<ICmsDocumentsResponseValidator> _mockCmsDocumentsResponseValidator;
        private readonly Mock<ITelemetryClient> _mockTelemetryClient;
        private readonly RefreshCaseOrchestrator _coordinatorOrchestrator;

        public RefreshCaseOrchestratorTests()
        {
            var fixture = new Fixture();
            _cmsAuthValues = fixture.Create<string>();
            _urn = fixture.Create<string>();
            _caseId = fixture.Create<long>();
            _correlationId = fixture.Create<Guid>();

            fixture.Create<Guid>();
            _payload = fixture.Build<CasePayload>()
                        .With(p => p.CmsAuthValues, _cmsAuthValues)
                        .With(p => p.CaseId, _caseId)
                        .With(p => p.Urn, _urn)
                        .With(p => p.CorrelationId, _correlationId)
                        .Create();
            _caseDocuments = fixture.Create<GetCaseDocumentsResponse>();
            _transactionId = $"[{_caseId}]";

            // (At least on a mac) this test suite crashes unless we control the format of CmsDocumentEntity.CmsOriginalFileName so that it
            //  matches the regex attribute that decorates it.
            fixture.Customize<CmsDocumentEntity>(c =>
                c.With(doc => doc.CmsOriginalFileName, $"{fixture.Create<string>()}.{fixture.Create<string>()[..3]}"));

            _trackerCmsDocuments = fixture.CreateMany<DocumentDelta>(11)
                .ToList();

            _deltaDocuments = new CaseDeltasEntity
            {
                CreatedCmsDocuments = _trackerCmsDocuments.Where(d => d.Document.Status == DocumentStatus.New).ToList(),
                UpdatedCmsDocuments = [.. fixture.Create<DocumentDelta[]>()],
                DeletedCmsDocuments = [.. fixture.Create<CmsDocumentEntity[]>()],
                CreatedPcdRequests = [],
                UpdatedPcdRequests = [],
                DeletedPcdRequests = [],
                CreatedDefendantsAndCharges = fixture.Create<DefendantsAndChargesEntity>(),
                UpdatedDefendantsAndCharges = fixture.Create<DefendantsAndChargesEntity>(),
                IsDeletedDefendantsAndCharges = false
            };
            var redactPdfResponse = fixture.CreateMany<RedactPdfResponse>().ToList();

            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<RefreshCaseOrchestrator>>();
            _mockTaskOrchestrationContext = new Mock<TaskOrchestrationContext>();
            _mockTaskOrchestrationEntityFeature = new Mock<TaskOrchestrationEntityFeature>();
            _mockTaskOrchestrationContext.Setup(c => c.Entities).Returns(_mockTaskOrchestrationEntityFeature.Object);
            _mockTaskOrchestrationContext.Setup(c => c.CreateReplaySafeLogger(nameof(RefreshCaseOrchestrator))).Returns(mockLogger.Object);
            _mockTelemetryClient = new Mock<ITelemetryClient>();
            _mockCmsDocumentsResponseValidator = new Mock<ICmsDocumentsResponseValidator>();

            _mockTaskOrchestrationEntityFeature.Setup(f => f.CallEntityAsync<CaseDeltasEntity>(
                It.Is<EntityInstanceId>(e => e.Name.Equals(nameof(CaseDurableEntity), StringComparison.CurrentCultureIgnoreCase) && e.Key == _transactionId),
                nameof(CaseDurableEntity.GetCaseDocumentChanges),
                It.IsAny<GetCaseDocumentsResponse>(),
                default))
                .ReturnsAsync(_deltaDocuments);

            mockConfiguration
                .Setup(config => config[ConfigKeys.CoordinatorOrchestratorTimeoutSecs])
                .Returns("300");

            mockConfiguration
                .Setup(config => config[ConfigKeys.CoordinatorSwitchoverCaseId])
                .Returns(int.MaxValue.ToString());

            mockConfiguration
                .Setup(config => config[ConfigKeys.CoordinatorSwitchoverModulo])
                .Returns(int.MaxValue.ToString());

            _mockTaskOrchestrationContext
                .Setup(context => context.GetInput<CasePayload>())
                .Returns(_payload);

            _mockTaskOrchestrationContext
                .Setup(context => context.InstanceId)
                .Returns(_transactionId);

            _mockTaskOrchestrationContext
                .Setup(context => context.CallActivityAsync<GetCaseDocumentsResponse>(nameof(GetCaseDocuments), It.IsAny<CasePayload>(), default))//It.IsAny<TaskOptions>()))
                .ReturnsAsync(_caseDocuments);

            _mockTaskOrchestrationContext
                .Setup(context => context.CallSubOrchestratorAsync<RefreshDocumentResult>(nameof(RefreshDocumentOrchestrator), It.IsAny<DocumentPayload>(), It.IsAny<TaskOptions>()))
                .Returns(Task.FromResult(fixture.Create<RefreshDocumentResult>()));

            _mockCmsDocumentsResponseValidator.Setup(validator => validator.Validate(It.IsAny<CmsDocumentDto[]>())).Returns(true);

            _coordinatorOrchestrator = new RefreshCaseOrchestrator(
                mockLogger.Object,
                mockConfiguration.Object,
                _mockCmsDocumentsResponseValidator.Object,
                _mockTelemetryClient.Object);
        }

        [Fact]
        public async Task Run_ThrowsWhenPayloadIsNull()
        {
            _mockTaskOrchestrationContext.Setup(context => context.GetInput<CasePayload>())
                .Returns(default(CasePayload));

            await Assert.ThrowsAsync<ArgumentException>(() => _coordinatorOrchestrator.Run(_mockTaskOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_Tracker_Initialises()
        {
            await _coordinatorOrchestrator.Run(_mockTaskOrchestrationContext.Object);

            _mockTaskOrchestrationEntityFeature.Verify(f => f.SignalEntityAsync(
                It.Is<EntityInstanceId>(e => e.Name.Equals(nameof(CaseDurableEntity), StringComparison.CurrentCultureIgnoreCase) && e.Key == _transactionId),
                nameof(CaseDurableEntity.Reset),
                default,
                default));
        }

        [Fact]
        public async Task Run_DoesntCallDocumentTasksWhenCaseDocumentsIsEmpty()
        {
            _mockTaskOrchestrationContext
                .Setup(context => context.CallActivityAsync<GetCaseDocumentsResponse>(nameof(GetCaseDocuments), It.IsAny<CasePayload>(), default))
                .ReturnsAsync(new GetCaseDocumentsResponse([], [], new DefendantsAndChargesListDto()));

            await _coordinatorOrchestrator.Run(_mockTaskOrchestrationContext.Object);

            _mockTaskOrchestrationContext.Verify(context => context.CallSubOrchestratorAsync(nameof(RefreshDocumentOrchestrator), It.IsAny<DocumentPayload>(), It.IsAny<TaskOptions>()), Times.Never());
        }

        [Fact]
        public async Task Run_CallsSubOrchestratorForEachNewOrChangedDocument()
        {
            var newCmsDocuments = _trackerCmsDocuments.Where(t => t.Document.Status == DocumentStatus.New).ToList();
            await _coordinatorOrchestrator.Run(_mockTaskOrchestrationContext.Object);

            foreach (var document in newCmsDocuments)
            {
                _mockTaskOrchestrationContext.Verify(context => context.CallSubOrchestratorAsync<RefreshDocumentResult>(
                    nameof(RefreshDocumentOrchestrator),
                    It.Is<DocumentPayload>(payload => payload.CaseId == _payload.CaseId && payload.DocumentId == document.Document.DocumentId),
                    It.IsAny<TaskOptions>()));
            }
        }

        [Fact]
        public async Task Run_DoesNotThrowWhenSubOrchestratorCallFails()
        {
            _mockTaskOrchestrationContext.Setup(
                context => context.CallSubOrchestratorAsync(nameof(RefreshDocumentOrchestrator), It.IsAny<CasePayload>(), It.IsAny<TaskOptions>()))
                    .ThrowsAsync(new Exception());

            try
            {
                await _coordinatorOrchestrator.Run(_mockTaskOrchestrationContext.Object);
            }
            catch (Exception)
            {
                Assert.True(false);
            }
        }

        [Fact]
        public async Task Run_Tracker_RegistersCompleted()
        {
            // Act
            await _coordinatorOrchestrator.Run(_mockTaskOrchestrationContext.Object);

            // Assert
            var arg = new SetCaseStatusPayload { UpdatedAt = It.IsAny<DateTime>(), Status = CaseRefreshStatus.Completed, FailedReason = null };

            _mockTaskOrchestrationEntityFeature.Verify(f => f.SignalEntityAsync(
                It.Is<EntityInstanceId>(e => e.Name.Equals(nameof(CaseDurableEntity), StringComparison.CurrentCultureIgnoreCase) && e.Key == _transactionId),
                nameof(CaseDurableEntity.SetCaseStatus),
                It.Is<SetCaseStatusPayload>(p => p.Status == arg.Status),
                default));
        }

        [Fact]
        public async Task Run_ThrowsExceptionWhenExceptionOccurs()
        {
            _mockTaskOrchestrationContext
                .Setup(context => context.CallActivityAsync<GetCaseDocumentsResponse>(nameof(GetCaseDocuments), It.IsAny<CasePayload>(), default))
                .ThrowsAsync(new Exception("Test Exception"));

            await Assert.ThrowsAsync<Exception>(() => _coordinatorOrchestrator.Run(_mockTaskOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_Tracker_RegistersFailedWhenExceptionOccurs()
        {
            // Arrange
            _mockTaskOrchestrationContext
                .Setup(context => context.CallActivityAsync<GetCaseDocumentsResponse>(nameof(GetCaseDocuments), It.IsAny<CasePayload>(), It.IsAny<TaskOptions>()))
                .ThrowsAsync(new Exception("Test Exception"));

            try
            {
                // Act
                await _coordinatorOrchestrator.Run(_mockTaskOrchestrationContext.Object);
                Assert.False(true);
            }
            catch
            {
                // Assert
                var arg = new SetCaseStatusPayload { UpdatedAt = It.IsAny<DateTime>(), Status = CaseRefreshStatus.Failed, FailedReason = "Test Exception" };

                _mockTaskOrchestrationEntityFeature.Verify(f => f.SignalEntityAsync(
                    It.Is<EntityInstanceId>(e => e.Name.Equals(nameof(CaseDurableEntity), StringComparison.CurrentCultureIgnoreCase) && e.Key == _transactionId),
                    nameof(CaseDurableEntity.SetCaseStatus),
                    It.Is<SetCaseStatusPayload>(p => p.Status == arg.Status && p.FailedReason.Equals(arg.FailedReason)),
                    default));
            }
        }
    }
}
