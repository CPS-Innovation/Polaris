﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Common.Wrappers;
using coordinator.Durable.Entity;
using coordinator.Functions;
using Common.Dto.Response.Documents;
using Common.Dto.Response.Document;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Case;
using coordinator.Functions.DurableEntity.Entity.Mapper;
using coordinator.Durable.Payloads.Domain;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Microsoft.DurableTask.Entities;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Client.Entities;
using coordinator.Domain;
using Microsoft.Azure.Functions.Worker;
using Common.Constants;
using Microsoft.Extensions.Configuration;
using Common.Configuration;
using Common.Services.BlobStorage;

namespace coordinator.tests.Domain.Response.Documents
{
    public class TrackerTests
    {
        private const string DocumentsContainer = "documents";
        private readonly Fixture _fixture;
        private readonly List<CmsDocumentDto> _cmsDocuments;
        private readonly List<PcdRequestDto> _pcdRequests;
        private readonly DefendantsAndChargesListDto _defendantsAndChargesList;
        private readonly string _pdfBlobName;
        private readonly GetCaseDocumentsResponse _synchroniseDocumentsArg;
        private readonly List<CmsDocumentEntity> _trackerCmsDocuments;
        private readonly List<PcdRequestEntity> _trackerPcdRequests;
        private readonly string _caseUrn;
        private readonly int _caseId;
        private readonly Guid _correlationId;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly Mock<TaskEntityContext> _mockDurableEntityContext;
        private readonly Mock<DurableEntityClient> _mockDurableEntityClient;
        private readonly Mock<DurableTaskClient> _mockDurableTaskClient;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IPolarisBlobStorageService> _mockPolarisBlobStorageService;
        private readonly Mock<ILogger<GetTracker>> _mockLogger;
        private readonly CaseDurableEntity _caseEntity;
        private readonly CaseDurableEntityState _caseDurableEntityState;
        private readonly CaseDurableEntityDocumentsState _caseDurableEntityDocumentsState;
        private readonly EntityMetadata<CaseDurableEntityState> _entityStateResponse;
        private readonly GetTracker _trackerStatus;

        public TrackerTests()
        {
            _fixture = new Fixture();
            _cmsDocuments = _fixture.CreateMany<CmsDocumentDto>(3).ToList();
            _pcdRequests = _fixture.CreateMany<PcdRequestDto>(2).ToList();
            _defendantsAndChargesList = _fixture.Create<DefendantsAndChargesListDto>();
            _correlationId = _fixture.Create<Guid>();
            _caseDurableEntityState = _fixture.Create<CaseDurableEntityState>();

            // (At least on a mac) this test suite crashes unless we control the format of CmsDocumentEntity.CmsOriginalFileName so that it
            //  matches the regex attribute that decorates it.
            _fixture.Customize<CmsDocumentEntity>(c =>
                c.With(doc => doc.CmsOriginalFileName, $"{_fixture.Create<string>()}.{_fixture.Create<string>()[..3]}"));
            _trackerCmsDocuments = _fixture.Create<List<CmsDocumentEntity>>();

            _trackerPcdRequests = _fixture.Create<List<PcdRequestEntity>>();
            _caseUrn = _fixture.Create<string>();
            _caseId = _fixture.Create<int>();

            _pdfBlobName = _fixture.Create<string>();
            _caseDurableEntityDocumentsState = new CaseDurableEntityDocumentsState
            {
                CmsDocuments = _trackerCmsDocuments,
                PcdRequests = _trackerPcdRequests,
            };
            _synchroniseDocumentsArg = new([.. _cmsDocuments], _pcdRequests.ToArray(), _defendantsAndChargesList);
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c[StorageKeys.BlobServiceContainerNameDocuments])
                .Returns(DocumentsContainer);
            _mockPolarisBlobStorageService = new Mock<IPolarisBlobStorageService>();
            _mockPolarisBlobStorageService.Setup(s => s.TryGetObjectAsync<CaseDurableEntityDocumentsState>(It.Is<BlobIdType>(b => b.CaseId == _caseId)))
                .ReturnsAsync(_caseDurableEntityDocumentsState);
            _caseEntity = new CaseDurableEntity(_mockConfiguration.Object, (_) => _mockPolarisBlobStorageService.Object);

            _entityStateResponse = new EntityMetadata<CaseDurableEntityState>(CaseDurableEntity.GetEntityId(_caseId), _caseDurableEntityState);
            _jsonConvertWrapper = _fixture.Create<JsonConvertWrapper>();

            _mockDurableEntityContext = new Mock<TaskEntityContext>();
            _mockDurableEntityClient = new Mock<DurableEntityClient>("name");
            _mockDurableTaskClient = new Mock<DurableTaskClient>("name");
            _mockDurableTaskClient.Setup(c => c.Entities)
                .Returns(_mockDurableEntityClient.Object);
            _mockLogger = new Mock<ILogger<GetTracker>>();

            _mockDurableEntityClient
                .Setup
                (
                    client =>
                        client.GetEntityAsync<CaseDurableEntityState>
                        (
                            It.Is<EntityInstanceId>(e => e.Name.Equals(nameof(CaseDurableEntity), StringComparison.CurrentCultureIgnoreCase) && e.Key == $"[{_caseId}]"),
                            true,
                            default
                        )
                )
                .ReturnsAsync(_entityStateResponse);

            _caseEntity = new CaseDurableEntity(_mockConfiguration.Object, (_) => _mockPolarisBlobStorageService.Object);
            _trackerStatus = new GetTracker(_jsonConvertWrapper, _mockConfiguration.Object, new CaseDurableEntityMapper(), (_) => _mockPolarisBlobStorageService.Object, _mockLogger.Object);
        }

        [Fact(Skip = "Cannot mock or instantiate TaskEntity<TState>")]
        public void Initialise_Initialises()
        {
            _caseEntity.Reset();
            _caseDurableEntityDocumentsState.CmsDocuments.Should().NotBeNull();
            _caseEntity.GetState().Status.Should().Be(CaseRefreshStatus.NotStarted);
        }

        [Fact]
        public void Tracker_Serialised_And_Deserialises()
        {
            var serialisedTracker = JsonSerializer.Serialize(_caseDurableEntityDocumentsState);
            var deserialisedTracker = JsonSerializer.Deserialize<CaseDurableEntityDocumentsState>(serialisedTracker);

            _caseDurableEntityDocumentsState.CmsDocuments[0].DocumentId.Should().Be(deserialisedTracker.CmsDocuments[0].DocumentId);
            _caseDurableEntityDocumentsState.PcdRequests[0].DocumentId.Should().Be(deserialisedTracker.PcdRequests[0].DocumentId);
        }

        [Fact(Skip = "Cannot mock or instantiate TaskEntity<TState>")]
        public async Task Initialisation_SetsDocumentStatusToNone()
        {
            await _caseEntity.GetCaseDocumentChanges(_synchroniseDocumentsArg);

            var document = _caseDurableEntityDocumentsState.CmsDocuments.Find(document => document.CmsDocumentId == _cmsDocuments.First().DocumentId);
            document?.Status.Should().Be(DocumentStatus.New);
        }

        [Fact(Skip = "Cannot mock or instantiate TaskEntity<TState>")]
        public void RegisterCompleted_RegistersCompleted()
        {
            _caseEntity.Reset();
            _caseEntity.SetCaseStatus(new SetCaseStatusPayload { UpdatedAt = DateTime.Now, Status = CaseRefreshStatus.Running, FailedReason = null });
            _caseEntity.SetCaseStatus(new SetCaseStatusPayload { UpdatedAt = DateTime.Now, Status = CaseRefreshStatus.Completed, FailedReason = null });

            var state = _caseEntity.GetState();
            state.Status.Should().Be(CaseRefreshStatus.Completed);
            state.Completed.Should().NotBeNull();
        }

        [Fact(Skip = "Cannot mock or instantiate TaskEntity<TState>")]
        public void RegisterFailed_RegistersFailed()
        {
            // Arrange
            _caseEntity.Reset();

            // Act
            _caseEntity.SetCaseStatus(new SetCaseStatusPayload { UpdatedAt = DateTime.Now, Status = CaseRefreshStatus.Running, FailedReason = null });
            _caseEntity.SetCaseStatus(new SetCaseStatusPayload { UpdatedAt = DateTime.Now, Status = CaseRefreshStatus.Failed, FailedReason = "exceptionMessage" });

            // Assert
            var state = _caseEntity.GetState();
            state.Status.Should().Be(CaseRefreshStatus.Failed);
            state.FailedReason.Should().Be("exceptionMessage");
        }

        [Fact(Skip = "Cannot mock TaskEntityDispatcher")]
        public async Task Run_Tracker_Dispatches()
        {
            var mockDurableEntityDispatcher = new Mock<TaskEntityDispatcher>();

            await CaseDurableEntity.RunEntityAsync(mockDurableEntityDispatcher.Object);

            mockDurableEntityDispatcher.Verify(context => context.DispatchAsync<CaseDurableEntity>());
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsOK()
        {
            // Arrange
            var message = new DefaultHttpContext().Request;
            message.Headers.Append(HttpHeaderKeys.CorrelationId, _correlationId.ToString());

            // Act
            var response = await _trackerStatus.HttpStart(message, _caseUrn, _caseId, _mockDurableTaskClient.Object);

            // Assert
            response.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsTrackerDto()
        {
            var message = new DefaultHttpContext().Request;
            message.Headers.Append(HttpHeaderKeys.CorrelationId, _correlationId.ToString());
            var response = await _trackerStatus.HttpStart(message, _caseUrn, _caseId, _mockDurableTaskClient.Object);

            var okObjectResult = response as OkObjectResult;

            okObjectResult?.Value.Should().BeOfType<TrackerDto>();
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsNotFoundIfEntityNotFound()
        {
            // Arrange
            var entityStateResponse = new EntityMetadata<CaseDurableEntityState>(CaseDurableEntity.GetEntityId(_caseId));
            _mockDurableEntityClient
                .Setup
                    (
                        client => client.GetEntityAsync<CaseDurableEntityState>
                        (
                            It.Is<EntityInstanceId>(e => e.Name.Equals(nameof(CaseDurableEntity), StringComparison.CurrentCultureIgnoreCase) && e.Key == $"[{_caseId}]"),
                            true,
                            default
                        )
                    )
                .ReturnsAsync(entityStateResponse);

            var message = new DefaultHttpContext().Request;
            message.Headers.Append(HttpHeaderKeys.CorrelationId, _correlationId.ToString());
            var response = await _trackerStatus.HttpStart(message, _caseUrn, _caseId, _mockDurableTaskClient.Object);

            response.Should().BeOfType<NotFoundObjectResult>();
        }

        #region SynchroniseDocument

        [Fact(Skip = "Cannot mock or instantiate TaskEntity<TState>")]
        public async Task SynchroniseDocument_CreatesNewDocumentsAndPcdRequests()
        {
            // Arrange
            var tracker = new CaseDurableEntity(_mockConfiguration.Object, (_) => _mockPolarisBlobStorageService.Object);
            tracker.Reset();

            // Act
            var deltas = await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);

            // Assert
            (await tracker.GetDurableEntityDocumentsStateAsync()).CmsDocuments.Count.Should().Be(_cmsDocuments.Count);
            deltas.CreatedCmsDocuments.Count.Should().Be(_cmsDocuments.Count);
            deltas.UpdatedCmsDocuments.Count.Should().Be(0);
            deltas.DeletedCmsDocuments.Count.Should().Be(0);
            deltas.CreatedPcdRequests.Count.Should().Be(_pcdRequests.Count);
            deltas.UpdatedPcdRequests.Count.Should().Be(0);
            deltas.DeletedPcdRequests.Count.Should().Be(0);
        }

        [Fact(Skip = "Cannot mock or instantiate TaskEntity<TState>")]
        public async Task SynchroniseDocument_NoChangesWithExistingDocumentAndVersionIds()
        {
            // Arrange
            var tracker = new CaseDurableEntity(_mockConfiguration.Object, (_) => _mockPolarisBlobStorageService.Object);
            tracker.Reset();
            await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            var documents = await tracker.GetDurableEntityDocumentsStateAsync();
            documents.CmsDocuments.ForEach(doc => doc.Status = DocumentStatus.Indexed);
            documents.PcdRequests.ForEach(pcd => pcd.Status = DocumentStatus.Indexed);

            // Act 
            var deltas = await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);

            // Assert
            documents = await tracker.GetDurableEntityDocumentsStateAsync();
            documents.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);
            deltas.CreatedCmsDocuments.Count.Should().Be(0);
            deltas.UpdatedCmsDocuments.Count.Should().Be(0);
            deltas.DeletedCmsDocuments.Count.Should().Be(0);
        }

        [Fact(Skip = "Cannot mock or instantiate TaskEntity<TState>")]
        public async Task SynchroniseDocument_IndexedCmsDocumentAreRetained()
        {
            // Arrange
            var tracker = new CaseDurableEntity(_mockConfiguration.Object, (_) => _mockPolarisBlobStorageService.Object);
            tracker.Reset();
            await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            var documents = await tracker.GetDurableEntityDocumentsStateAsync();
            documents.CmsDocuments.ForEach(doc => doc.Status = DocumentStatus.Indexed);
            documents.PcdRequests.ForEach(pcd => pcd.Status = DocumentStatus.Indexed);

            // Act 
            var deltas = await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);

            // Assert
            documents = await tracker.GetDurableEntityDocumentsStateAsync();
            documents.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);
            deltas.CreatedCmsDocuments.Count.Should().Be(0);
            deltas.UpdatedCmsDocuments.Count.Should().Be(0);
            deltas.DeletedCmsDocuments.Count.Should().Be(0);
        }

        [Fact(Skip = "Cannot mock or instantiate TaskEntity<TState>")]
        public async Task SynchroniseDocument_ChangesWithUpdatedDocumentAndVersionIds()
        {
            // Arrange
            var tracker = new CaseDurableEntity(_mockConfiguration.Object, (_) => _mockPolarisBlobStorageService.Object);
            tracker.Reset();
            await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            var documents = await tracker.GetDurableEntityDocumentsStateAsync();
            documents.CmsDocuments.ForEach(doc => doc.Status = DocumentStatus.Indexed);
            documents.PcdRequests.ForEach(pcd => pcd.Status = DocumentStatus.Indexed);

            // Act 
            _synchroniseDocumentsArg.CmsDocuments[1].VersionId = 111111111;
            _synchroniseDocumentsArg.CmsDocuments[2].VersionId = 222222222;
            var deltas = await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);

            // Assert
            documents = await tracker.GetDurableEntityDocumentsStateAsync();
            documents.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);
            deltas.CreatedCmsDocuments.Count.Should().Be(0);
            deltas.UpdatedCmsDocuments.Count.Should().Be(2);
            deltas.DeletedCmsDocuments.Count.Should().Be(0);
        }

        [Fact(Skip = "Cannot mock or instantiate TaskEntity<TState>")]
        public async Task SynchroniseDocument_ChangesWithDeletedDocuments()
        {
            // Arrange
            var tracker = new CaseDurableEntity(_mockConfiguration.Object, (_) => _mockPolarisBlobStorageService.Object);
            tracker.Reset();
            await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            var documents = await tracker.GetDurableEntityDocumentsStateAsync();
            documents.CmsDocuments.ForEach(doc => doc.Status = DocumentStatus.Indexed);
            documents.PcdRequests.ForEach(doc => doc.Status = DocumentStatus.Indexed);
            var synchroniseDocumentsArg = new GetCaseDocumentsResponse(_cmsDocuments.Take(1).ToArray(), _pcdRequests.Take(1).ToArray(), null);

            // Act 
            var deltas = await tracker.GetCaseDocumentChanges(synchroniseDocumentsArg);

            // Assert
            documents = await tracker.GetDurableEntityDocumentsStateAsync();
            documents.CmsDocuments.Count.Should().Be(1);
            deltas.CreatedCmsDocuments.Count.Should().Be(0);
            deltas.UpdatedCmsDocuments.Count.Should().Be(0);
            deltas.DeletedCmsDocuments.Count.Should().Be(2);
            deltas.CreatedPcdRequests.Count.Should().Be(0);
            deltas.UpdatedPcdRequests.Count.Should().Be(0);
            deltas.DeletedPcdRequests.Count.Should().Be(1);
        }

        [Fact(Skip = "Cannot mock or instantiate TaskEntity<TState>")]
        public async Task RegisterDocumentIds_TheNextDaysRun_DocumentsTheSame_ReturnsNothingToEvaluate()
        {
            _caseEntity.Reset();
            await _caseEntity.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            _caseDurableEntityDocumentsState.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);

            var newDaysDocuments = new CmsDocumentDto[3];
            _cmsDocuments.CopyTo(newDaysDocuments);

            _caseEntity.Reset();
            await _caseEntity.GetCaseDocumentChanges(_synchroniseDocumentsArg);

            using (new AssertionScope())
            {
                _caseDurableEntityDocumentsState.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);
            }
        }

        [Fact(Skip = "Cannot mock or instantiate TaskEntity<TState>")]
        public async Task RegisterDocumentIds_TheNextDaysRun_DocumentsNotTheSame_ReturnsRecordsToEvaluate()
        {
            _caseEntity.Reset();
            await _caseEntity.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            _caseDurableEntityDocumentsState.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);
            _caseDurableEntityDocumentsState.PcdRequests.Count.Should().Be(_pcdRequests.Count);
            _caseDurableEntityDocumentsState.DefendantsAndCharges.Should().NotBeNull();

            var newDaysDocuments = new List<CmsDocumentDto> { _cmsDocuments.First() };
            ////only one document in today's run, the next two should be removed from the tracker and in the evaluation results

            var newDaysDocumentIdsArg = new GetCaseDocumentsResponse([.. newDaysDocuments], Array.Empty<PcdRequestDto>(), null);

            _caseEntity.Reset();
            await _caseEntity.GetCaseDocumentChanges(newDaysDocumentIdsArg);

            using (new AssertionScope())
            {
                _caseDurableEntityDocumentsState.CmsDocuments.Count.Should().Be(1);
            }
        }

        [Fact(Skip = "Cannot mock or instantiate TaskEntity<TState>")]
        public async Task RegisterDocumentIds_TheNextDaysRun_DocumentsTheSameExceptForANewVersionOfOneDoc_ReturnsOneRecordToEvaluate()
        {
            _caseEntity.Reset();
            await _caseEntity.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            _caseDurableEntityDocumentsState.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);

            var nextDocs = new CmsDocumentDto[3];
            _cmsDocuments.CopyTo(nextDocs);
            nextDocs[1].VersionId += 1;

            _caseEntity.Reset();

            await _caseEntity.GetCaseDocumentChanges(new GetCaseDocumentsResponse(nextDocs, Array.Empty<PcdRequestDto>(), null));

            using (new AssertionScope())
            {
                _caseDurableEntityDocumentsState.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);
                var newVersion = _caseDurableEntityDocumentsState.CmsDocuments.Find(x => x.CmsDocumentId == nextDocs[1].DocumentId);
                newVersion.VersionId.Should().Be(nextDocs[1].VersionId);
            }
        }

        [Fact(Skip = "Cannot mock or instantiate TaskEntity<TState>")]
        public async Task RegisterDocumentIds_TheNextDaysRun_OneDocumentRemovedAndOneANewVersion_ReturnsTwoRecordToEvaluate()
        {
            _caseEntity.Reset();
            await _caseEntity.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            _caseDurableEntityDocumentsState.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);

            var newDaysDocuments = new List<CmsDocumentDto>
            {
                _cmsDocuments[1],
                _cmsDocuments[2]
            };

            var documentRemovedFromCmsId = _cmsDocuments[0].DocumentId;
            var originalVersionId = newDaysDocuments[0].VersionId;
            var newVersionId = originalVersionId + 1;
            newDaysDocuments[0].VersionId = newVersionId;
            var modifiedDocumentId = newDaysDocuments[0].DocumentId;

            var unmodifiedDocumentId = newDaysDocuments[1].DocumentId;
            var unmodifiedDocumentVersionId = newDaysDocuments[1].VersionId;

            var newDaysDocumentIdsArg = new GetCaseDocumentsResponse([.. newDaysDocuments], _pcdRequests.ToArray(), null);

            _caseEntity.Reset();
            await _caseEntity.GetCaseDocumentChanges(newDaysDocumentIdsArg);

            using (new AssertionScope())
            {
                _caseDurableEntityDocumentsState.CmsDocuments.Count.Should().Be(2);
                var newVersion = _caseDurableEntityDocumentsState.CmsDocuments.Find(x => x.CmsDocumentId == modifiedDocumentId);
                var unmodifiedDocument = _caseDurableEntityDocumentsState.CmsDocuments.Find(x => x.CmsDocumentId == unmodifiedDocumentId);

                newVersion.Should().NotBeNull();
                newVersion?.VersionId.Should().Be(newVersionId);

                unmodifiedDocument.Should().NotBeNull();
                unmodifiedDocument?.VersionId.Should().Be(unmodifiedDocumentVersionId);

                var searchResultForDocumentRemovedFromCms = _caseDurableEntityDocumentsState.CmsDocuments.Find(x => x.CmsDocumentId == documentRemovedFromCmsId);
                searchResultForDocumentRemovedFromCms.Should().BeNull();
            }
        }
        #endregion
    }
}