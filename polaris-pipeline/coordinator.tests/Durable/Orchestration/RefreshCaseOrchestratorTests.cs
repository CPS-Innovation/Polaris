﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Common.Dto.Case.PreCharge;
using Common.Dto.Case;
using Common.Dto.Document;
using Common.Dto.Response;
using Common.Dto.Tracker;
using coordinator.Constants;
using coordinator.Domain.Exceptions;
using coordinator.Durable.Activity;
using coordinator.Durable.Orchestration;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using coordinator.Durable.Entity;
using Common.Telemetry;
using coordinator.Validators;
using coordinator.Durable.Payloads;
using coordinator.Durable.Payloads.Domain;
using Newtonsoft.Json;

namespace coordinator.tests.Durable.Orchestration
{
    public class RefreshCaseOrchestratorTests
    {
        private readonly CaseOrchestrationPayload _payload;
        private readonly string _cmsAuthValues;
        private readonly long _caseId;
        private readonly string _urn;
        private readonly string _documentId;
        private readonly Guid _correlationId;
        private readonly (CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) _caseDocuments;
        private readonly string _transactionId;
        private readonly List<(CmsDocumentEntity, DocumentDeltaType)> _trackerCmsDocuments;
        private readonly CaseDeltasEntity _deltaDocuments;

        private readonly Mock<IDurableOrchestrationContext> _mockDurableOrchestrationContext;
        private readonly Mock<ICaseDurableEntity> _mockCaseEntity;
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
            _documentId = fixture.Create<string>();
            fixture.Create<Guid>();
            var durableRequest = new DurableHttpRequest(HttpMethod.Post, new Uri("https://www.google.co.uk"));
            _payload = fixture.Build<CaseOrchestrationPayload>()
                        .With(p => p.CmsAuthValues, _cmsAuthValues)
                        .With(p => p.CaseId, _caseId)
                        .With(p => p.Urn, _urn)
                        .With(p => p.CorrelationId, _correlationId)
                        .With(p => p.DocumentId, _documentId)
                        .Create();
            _caseDocuments = fixture.Create<(CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>();

            _transactionId = $"[{_caseId}]";

            // (At least on a mac) this test suite crashes unless we control the format of CmsDocumentEntity.CmsOriginalFileName so that it
            //  matches the regex attribute that decorates it.
            fixture.Customize<CmsDocumentEntity>(c =>
                c.With(doc => doc.CmsOriginalFileName, $"{fixture.Create<string>()}.{fixture.Create<string>().Substring(0, 3)}"));

            _trackerCmsDocuments = fixture.CreateMany<(CmsDocumentEntity, DocumentDeltaType)>(11)
                .ToList();

            _deltaDocuments = new CaseDeltasEntity
            {
                CreatedCmsDocuments = _trackerCmsDocuments.Where(d => d.Item1.Status == DocumentStatus.New).ToList(),
                UpdatedCmsDocuments = fixture.Create<(CmsDocumentEntity, DocumentDeltaType)[]>().ToList(),
                DeletedCmsDocuments = fixture.Create<CmsDocumentEntity[]>().ToList(),
                CreatedPcdRequests = new List<PcdRequestEntity> { },
                UpdatedPcdRequests = new List<PcdRequestEntity> { },
                DeletedPcdRequests = new List<PcdRequestEntity> { },
                CreatedDefendantsAndCharges = fixture.Create<DefendantsAndChargesEntity>(),
                UpdatedDefendantsAndCharges = fixture.Create<DefendantsAndChargesEntity>(),
                IsDeletedDefendantsAndCharges = false
            };
            var redactPdfResponse = fixture.CreateMany<RedactPdfResponse>().ToList();

            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<RefreshCaseOrchestrator>>();
            _mockDurableOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockCaseEntity = new Mock<ICaseDurableEntity>();
            _mockTelemetryClient = new Mock<ITelemetryClient>();
            _mockCmsDocumentsResponseValidator = new Mock<ICmsDocumentsResponseValidator>();

            mockConfiguration
                .Setup(config => config[ConfigKeys.CoordinatorOrchestratorTimeoutSecs])
                .Returns("300");

            mockConfiguration
                .Setup(config => config[ConfigKeys.CoordinatorSwitchoverCaseId])
                .Returns(int.MaxValue.ToString());

            mockConfiguration
                .Setup(config => config[ConfigKeys.CoordinatorSwitchoverModulo])
                .Returns(int.MaxValue.ToString());

            _mockCaseEntity
                .Setup(tracker => tracker.GetCaseDocumentChanges(((CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges))It.IsAny<object>()))
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
                .Setup(context => context.CreateEntityProxy<ICaseDurableEntity>(It.Is<EntityId>(e => e.EntityName == nameof(CaseDurableEntity).ToLower() && e.EntityKey == _transactionId)))
                .Returns(_mockCaseEntity.Object);

            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<(CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>(nameof(GetCaseDocuments), It.IsAny<GetCaseDocumentsActivityPayload>()))
                .ReturnsAsync(_caseDocuments);

            _mockDurableOrchestrationContext
                .Setup(context => context.CallSubOrchestratorAsync<RefreshDocumentResult>(nameof(RefreshDocumentOrchestrator), It.IsAny<string>(), It.IsAny<CaseDocumentOrchestrationPayload>()))
                .Returns(Task.FromResult(fixture.Create<RefreshDocumentResult>()));

            var durableResponse = new DurableHttpResponse(HttpStatusCode.OK, content: JsonConvert.SerializeObject(redactPdfResponse));
            _mockDurableOrchestrationContext.Setup(context => context.CallHttpAsync(durableRequest)).ReturnsAsync(durableResponse);

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
                .Setup(tracker => tracker.GetCaseDocumentChanges(It.IsAny<(CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>()))
                .ReturnsAsync(new CaseDeltasEntity
                {
                    CreatedCmsDocuments = new List<(CmsDocumentEntity, DocumentDeltaType)>(),
                    UpdatedCmsDocuments = new List<(CmsDocumentEntity, DocumentDeltaType)>(),
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
            List<(CmsDocumentEntity, DocumentDeltaType)> newCmsDocuments = _trackerCmsDocuments.Where(t => t.Item1.Status == DocumentStatus.New).ToList();
            await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            foreach (var document in newCmsDocuments)
            {
                _mockDurableOrchestrationContext.Verify(context => context.CallSubOrchestratorAsync<RefreshDocumentResult>
                (
                    It.IsIn<string>(new[] { nameof(RefreshDocumentOrchestrator), nameof(RefreshDocumentOrchestrator) }),
                    It.IsAny<string>(),
                    It.Is<CaseDocumentOrchestrationPayload>
                    (
                        payload =>
                            payload.CaseId == _payload.CaseId &&
                            (
                                (payload.CmsDocumentTracker != null && payload.CmsDocumentTracker.CmsDocumentId == document.Item1.CmsDocumentId) ||
                                (payload.DefendantAndChargesTracker != null && payload.DefendantAndChargesTracker.CmsDocumentId == document.Item1.CmsDocumentId)
                            ))
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
            // Arrange
            _mockCaseEntity.Setup(t => t.AllDocumentsFailed()).ReturnsAsync(true);

            // Act + Assert
            await Assert.ThrowsAsync<CaseOrchestrationException>(() => _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object));
        }

        [Fact]
        public async Task Run_Tracker_RegistersCompleted()
        {
            // Act
            await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);

            // Assert
            var arg = (It.IsAny<DateTime>(), CaseRefreshStatus.Completed, It.IsAny<string>());
            _mockCaseEntity.Verify(tracker => tracker.SetCaseStatus(arg));
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
            // Arrange
            _mockDurableOrchestrationContext
                .Setup(context => context.CallActivityAsync<(CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>(nameof(GetCaseDocuments), It.IsAny<GetCaseDocumentsActivityPayload>()))
                .ThrowsAsync(new Exception("Test Exception"));

            try
            {
                // Act
                await _coordinatorOrchestrator.Run(_mockDurableOrchestrationContext.Object);
                Assert.False(true);
            }
            catch
            {
                // Assert
                var arg = (It.IsAny<DateTime>(), CaseRefreshStatus.Failed, "Test Exception");
                _mockCaseEntity.Verify(tracker => tracker.SetCaseStatus(arg));
            }
        }
    }
}
