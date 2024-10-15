﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Common.Wrappers;
using coordinator.Durable.Entity;
using coordinator.Functions;
using Common.Dto.Tracker;
using Common.Dto.Response.Document;
using Common.Dto.Response.Document.FeatureFlags;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Case;
using Newtonsoft.Json;
using coordinator.Functions.DurableEntity.Entity.Mapper;
using coordinator.Durable.Payloads.Domain;
using Microsoft.AspNetCore.Http;

namespace coordinator.tests.Domain.Tracker
{
    public class TrackerTests
    {
        private readonly Fixture _fixture;
        private readonly string _transactionId;
        private readonly List<CmsDocumentDto> _cmsDocuments;
        private readonly List<PcdRequestDto> _pcdRequests;
        private readonly DefendantsAndChargesListDto _defendantsAndChargesList;
        private readonly string _pdfBlobName;
        private readonly (CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) _synchroniseDocumentsArg;
        private readonly List<CmsDocumentEntity> _trackerCmsDocuments;
        private readonly List<PcdRequestEntity> _trackerPcdRequests;
        private readonly string _caseUrn;
        private readonly int _caseId;
        private readonly Guid _correlationId;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly Mock<IDurableEntityContext> _mockDurableEntityContext;
        private readonly Mock<IDurableEntityClient> _mockDurableEntityClient;
        private readonly Mock<ILogger<GetTracker>> _mockLogger;
        private readonly CaseDurableEntity _caseEntity;
        private readonly EntityStateResponse<CaseDurableEntity> _entityStateResponse;
        private readonly GetTracker _trackerStatus;

        public TrackerTests()
        {
            _fixture = new Fixture();
            _transactionId = _fixture.Create<string>();
            _cmsDocuments = _fixture.CreateMany<CmsDocumentDto>(3).ToList();
            _pcdRequests = _fixture.CreateMany<PcdRequestDto>(2).ToList();
            _defendantsAndChargesList = _fixture.Create<DefendantsAndChargesListDto>();
            _correlationId = _fixture.Create<Guid>();

            // (At least on a mac) this test suite crashes unless we control the format of CmsDocumentEntity.CmsOriginalFileName so that it
            //  matches the regex attribute that decorates it.
            _fixture.Customize<CmsDocumentEntity>(c =>
                c.With(doc => doc.CmsOriginalFileName, $"{_fixture.Create<string>()}.{_fixture.Create<string>().Substring(0, 3)}"));
            _trackerCmsDocuments = _fixture.Create<List<CmsDocumentEntity>>();

            _trackerPcdRequests = _fixture.Create<List<PcdRequestEntity>>();
            _caseUrn = _fixture.Create<string>();
            _caseId = _fixture.Create<int>();
            _caseEntity = _fixture.Create<CaseDurableEntity>();

            _pdfBlobName = _fixture.Create<string>();

            _synchroniseDocumentsArg = (_cmsDocuments.ToArray(), _pcdRequests.ToArray(), _defendantsAndChargesList);
            _entityStateResponse = new EntityStateResponse<CaseDurableEntity>() { EntityExists = true, EntityState = _caseEntity };
            _jsonConvertWrapper = _fixture.Create<JsonConvertWrapper>();

            _mockDurableEntityContext = new Mock<IDurableEntityContext>();
            _mockDurableEntityClient = new Mock<IDurableEntityClient>();
            _mockLogger = new Mock<ILogger<GetTracker>>();

            _mockDurableEntityClient
                .Setup
                (
                    client =>
                        client.ReadEntityStateAsync<CaseDurableEntity>
                        (
                            It.Is<EntityId>(e => e.EntityName == nameof(CaseDurableEntity).ToLower() && e.EntityKey == $"[{_caseId}]"),
                            null,
                            null
                        )
                )
                .ReturnsAsync(_entityStateResponse);

            _caseEntity = new CaseDurableEntity();
            _caseEntity.TransactionId = _transactionId;
            _caseEntity.CmsDocuments = _trackerCmsDocuments;
            _caseEntity.PcdRequests = _trackerPcdRequests;
            _trackerStatus = new GetTracker(_jsonConvertWrapper, new CaseDurableEntityMapper(), _mockLogger.Object);
        }

        [Fact]
        public void Initialise_Initialises()
        {
            _caseEntity.Reset(_transactionId);

            _caseEntity.TransactionId.Should().Be(_transactionId);
            _caseEntity.CmsDocuments.Should().NotBeNull();
            _caseEntity.Status.Should().Be(CaseRefreshStatus.NotStarted);
        }

        [Fact]
        public void Tracker_Serialised_And_Deserialises()
        {
            var serialisedTracker = JsonConvert.SerializeObject(_caseEntity);
            var deserialisedTracker = JsonConvert.DeserializeObject<CaseDurableEntity>(serialisedTracker);

            _caseEntity.CmsDocuments[0].DocumentId.Should().Be(deserialisedTracker.CmsDocuments[0].DocumentId);
            _caseEntity.PcdRequests[0].DocumentId.Should().Be(deserialisedTracker.PcdRequests[0].DocumentId);
            deserialisedTracker.TransactionId.Should().Be(_transactionId);
        }

        [Fact]
        public async Task Initialisation_SetsDocumentStatusToNone()
        {
            _caseEntity.Reset(_transactionId);
            await _caseEntity.GetCaseDocumentChanges(_synchroniseDocumentsArg);

            var document = _caseEntity.CmsDocuments.Find(document => document.CmsDocumentId == _cmsDocuments.First().DocumentId);
            document?.Status.Should().Be(DocumentStatus.New);
        }

        [Fact]
        public void RegisterCompleted_RegistersCompleted()
        {
            _caseEntity.Reset(_transactionId);
            _caseEntity.SetCaseStatus((DateTime.Now, CaseRefreshStatus.Running, null));
            _caseEntity.SetCaseStatus((DateTime.Now, CaseRefreshStatus.Completed, null));

            _caseEntity.Status.Should().Be(CaseRefreshStatus.Completed);
            _caseEntity.Completed.Should().NotBeNull();
        }

        [Fact]
        public void RegisterFailed_RegistersFailed()
        {
            // Arrange
            _caseEntity.Reset(_transactionId);

            // Act
            _caseEntity.SetCaseStatus((DateTime.Now, CaseRefreshStatus.Running, null));
            _caseEntity.SetCaseStatus((It.IsAny<DateTime>(), CaseRefreshStatus.Failed, "exceptionMessage"));

            // Assert
            _caseEntity.Status.Should().Be(CaseRefreshStatus.Failed);
            _caseEntity.FailedReason.Should().Be("exceptionMessage");
        }

        [Fact]
        public async Task AllDocumentsFailed_ReturnsTrueIfAllDocumentsFailed()
        {
            _caseEntity.CmsDocuments = new List<CmsDocumentEntity> {
                new(_fixture.Create<long>(),  _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), true, true, _fixture.Create<int?>(), _fixture.Create<string>(), null, _fixture.Create<PresentationFlagsDto>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<string>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<string>(), _fixture.Create<string>()) { Status = DocumentStatus.UnableToConvertToPdf},
                new(_fixture.Create<long>(),  _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(),  _fixture.Create<string>(), true, true, _fixture.Create<int?>(), _fixture.Create<string>(), null, _fixture.Create<PresentationFlagsDto>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<string>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<string>(), _fixture.Create<string>()) { Status = DocumentStatus.UnableToConvertToPdf}
            };
            _caseEntity.PcdRequests = new List<PcdRequestEntity>();
            _caseEntity.DefendantsAndCharges = new DefendantsAndChargesEntity { Status = DocumentStatus.UnableToConvertToPdf };

            var output = await _caseEntity.AllDocumentsFailed();

            output.Should().BeTrue();
        }

        [Fact]
        public async Task AllDocumentsFailed_ReturnsFalseIfAllDocumentsHaveNotFailed()
        {
            _caseEntity.CmsDocuments = new List<CmsDocumentEntity> {
                new(_fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), true, true, _fixture.Create<int?>(), _fixture.Create<string>(), null,  _fixture.Create<PresentationFlagsDto>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<string>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<string>(), _fixture.Create<string>()) { Status = DocumentStatus.UnableToConvertToPdf},
                new(_fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), true, true, _fixture.Create<int?>(), _fixture.Create<string>(), null, _fixture.Create<PresentationFlagsDto>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<string>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<string>(), _fixture.Create<string>()) { Status = DocumentStatus.UnableToConvertToPdf},
                new(_fixture.Create<long>(), _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), true, true, _fixture.Create<int?>(), _fixture.Create<string>(), null, _fixture.Create<PresentationFlagsDto>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<string>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<string>(), _fixture.Create<string>()) { Status = DocumentStatus.PdfUploadedToBlob},
            };
            _caseEntity.PcdRequests = new List<PcdRequestEntity>();
            _caseEntity.DefendantsAndCharges = new DefendantsAndChargesEntity { Status = DocumentStatus.Indexed };

            var output = await _caseEntity.AllDocumentsFailed();

            output.Should().BeFalse();
        }

        [Fact]
        public async Task Run_Tracker_Dispatches()
        {
            await CaseDurableEntity.Run(_mockDurableEntityContext.Object);

            _mockDurableEntityContext.Verify(context => context.DispatchAsync<CaseDurableEntity>());
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsOK()
        {
            // Arrange
            var message = new DefaultHttpContext().Request;
            message.Headers.Add("Correlation-Id", _correlationId.ToString());

            // Act
            var response = await _trackerStatus.HttpStart(message, _caseUrn, _caseId, _mockDurableEntityClient.Object);

            // Assert
            response.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsTrackerDto()
        {
            var message = new DefaultHttpContext().Request;
            message.Headers.Add("Correlation-Id", _correlationId.ToString());
            var response = await _trackerStatus.HttpStart(message, _caseUrn, _caseId, _mockDurableEntityClient.Object);

            var okObjectResult = response as OkObjectResult;

            okObjectResult?.Value.Should().BeOfType<TrackerDto>();
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsNotFoundIfEntityNotFound()
        {
            // Arrange
            var entityStateResponse = new EntityStateResponse<CaseDurableEntity>() { EntityExists = false };
            _mockDurableEntityClient
                .Setup
                    (
                        client => client.ReadEntityStateAsync<CaseDurableEntity>
                        (
                            It.Is<EntityId>(e => e.EntityName == nameof(CaseDurableEntity).ToLower() && e.EntityKey == $"[{_caseId}]"),
                            null,
                            null
                        )
                    )
                .ReturnsAsync(entityStateResponse);

            var message = new DefaultHttpContext().Request;
            message.Headers.Add("Correlation-Id", _correlationId.ToString());
            var response = await _trackerStatus.HttpStart(message, _caseUrn, _caseId, _mockDurableEntityClient.Object);

            response.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsBadRequestIfCorrelationIdNotFound()
        {
            var entityStateResponse = new EntityStateResponse<CaseDurableEntity>() { EntityExists = false };
            _mockDurableEntityClient.Setup(
                    client => client.ReadEntityStateAsync<CaseDurableEntity>(
                        It.Is<EntityId>(e => e.EntityName == nameof(CaseDurableEntity).ToLower() && e.EntityKey == _caseId.ToString()),
                        null, null))
                .ReturnsAsync(entityStateResponse);

            var message = new DefaultHttpContext().Request;
            //message.Headers.Add("Correlation-Id", _correlationId.ToString());
            var response = await _trackerStatus.HttpStart(message, _caseUrn, _caseId, _mockDurableEntityClient.Object);

            response.Should().BeOfType<StatusCodeResult>().Which.StatusCode.Should().Be(400);
        }

        #region SynchroniseDocument

        [Fact]
        public async Task SynchroniseDocument_CreatesNewDocumentsAndPcdRequests()
        {
            // Arrange
            CaseDurableEntity tracker = new CaseDurableEntity();
            tracker.Reset(_transactionId);

            // Act
            var deltas = await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);

            // Assert
            tracker.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);
            deltas.CreatedCmsDocuments.Count.Should().Be(_cmsDocuments.Count);
            deltas.UpdatedCmsDocuments.Count.Should().Be(0);
            deltas.DeletedCmsDocuments.Count.Should().Be(0);
            deltas.CreatedPcdRequests.Count.Should().Be(_pcdRequests.Count);
            deltas.UpdatedPcdRequests.Count.Should().Be(0);
            deltas.DeletedPcdRequests.Count.Should().Be(0);
            deltas.Any().Should().BeTrue();
        }

        [Fact]
        public async Task SynchroniseDocument_NoChangesWithExistingDocumentAndVersionIds()
        {
            // Arrange
            CaseDurableEntity tracker = new CaseDurableEntity();
            tracker.Reset(_transactionId);
            await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            tracker.CmsDocuments.ForEach(doc => doc.Status = DocumentStatus.Indexed);
            tracker.PcdRequests.ForEach(pcd => pcd.Status = DocumentStatus.Indexed);

            // Act 
            var deltas = await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);

            // Assert
            tracker.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);
            deltas.CreatedCmsDocuments.Count.Should().Be(0);
            deltas.UpdatedCmsDocuments.Count.Should().Be(0);
            deltas.DeletedCmsDocuments.Count.Should().Be(0);
            deltas.Any().Should().BeFalse();
        }

        [Fact]
        public async Task SynchroniseDocument_IndexedCmsDocumentAreRetained()
        {
            // Arrange
            CaseDurableEntity tracker = new CaseDurableEntity();
            tracker.Reset(_transactionId);
            await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            tracker.CmsDocuments.ForEach(doc => doc.Status = DocumentStatus.Indexed);
            tracker.PcdRequests.ForEach(pcd => pcd.Status = DocumentStatus.Indexed);

            // Act 
            var deltas = await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);

            // Assert
            tracker.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);
            deltas.CreatedCmsDocuments.Count.Should().Be(0);
            deltas.UpdatedCmsDocuments.Count.Should().Be(0);
            deltas.DeletedCmsDocuments.Count.Should().Be(0);
            deltas.Any().Should().BeFalse();
        }

        [Fact]
        public async Task SynchroniseDocument_ChangesWithUpdatedDocumentAndVersionIds()
        {
            // Arrange
            CaseDurableEntity tracker = new CaseDurableEntity();
            tracker.Reset(_transactionId);
            await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            tracker.CmsDocuments.ForEach(doc => doc.Status = DocumentStatus.Indexed);
            tracker.PcdRequests.ForEach(pcd => pcd.Status = DocumentStatus.Indexed);

            // Act 
            _synchroniseDocumentsArg.CmsDocuments[1].VersionId = 111111111;
            _synchroniseDocumentsArg.CmsDocuments[2].VersionId = 222222222;
            var deltas = await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);

            // Assert
            tracker.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);
            deltas.CreatedCmsDocuments.Count.Should().Be(0);
            deltas.UpdatedCmsDocuments.Count.Should().Be(2);
            deltas.DeletedCmsDocuments.Count.Should().Be(0);
            deltas.Any().Should().BeTrue();
        }

        [Fact]
        public async Task SynchroniseDocument_ChangesWithDeletedDocuments()
        {
            // Arrange
            var tracker = new CaseDurableEntity();
            tracker.Reset(_transactionId);
            await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            tracker.CmsDocuments.ForEach(doc => doc.Status = DocumentStatus.Indexed);
            tracker.PcdRequests.ForEach(doc => doc.Status = DocumentStatus.Indexed);
            (CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) synchroniseDocumentsArg
                = new(_cmsDocuments.Take(1).ToArray(), _pcdRequests.Take(1).ToArray(), null);

            // Act 
            var deltas = await tracker.GetCaseDocumentChanges(synchroniseDocumentsArg);

            // Assert
            tracker.CmsDocuments.Count.Should().Be(1);
            deltas.CreatedCmsDocuments.Count.Should().Be(0);
            deltas.UpdatedCmsDocuments.Count.Should().Be(0);
            deltas.DeletedCmsDocuments.Count.Should().Be(2);
            deltas.CreatedPcdRequests.Count.Should().Be(0);
            deltas.UpdatedPcdRequests.Count.Should().Be(0);
            deltas.DeletedPcdRequests.Count.Should().Be(1);
            deltas.Any().Should().BeTrue();
        }

        [Fact]
        public async Task RegisterDocumentIds_TheNextDaysRun_DocumentsTheSame_ReturnsNothingToEvaluate()
        {
            _caseEntity.Reset(_transactionId);
            await _caseEntity.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            _caseEntity.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);

            var newDaysDocuments = new CmsDocumentDto[3];
            _cmsDocuments.CopyTo(newDaysDocuments);

            _caseEntity.Reset(_transactionId);
            await _caseEntity.GetCaseDocumentChanges(_synchroniseDocumentsArg);

            using (new AssertionScope())
            {
                _caseEntity.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);
            }
        }

        [Fact]
        public async Task RegisterDocumentIds_TheNextDaysRun_DocumentsNotTheSame_ReturnsRecordsToEvaluate()
        {
            _caseEntity.Reset(_transactionId);
            await _caseEntity.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            _caseEntity.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);
            _caseEntity.PcdRequests.Count.Should().Be(_pcdRequests.Count);
            _caseEntity.DefendantsAndCharges.Should().NotBeNull();

            var newDaysDocuments = new List<CmsDocumentDto> { _cmsDocuments.First() };
            ////only one document in today's run, the next two should be removed from the tracker and in the evaluation results

            (CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) newDaysDocumentIdsArg =
                new(newDaysDocuments.ToArray(), Array.Empty<PcdRequestDto>(), null);

            _caseEntity.Reset(_transactionId);
            await _caseEntity.GetCaseDocumentChanges(newDaysDocumentIdsArg);

            using (new AssertionScope())
            {
                _caseEntity.CmsDocuments.Count.Should().Be(1);
            }
        }

        [Fact]
        public async Task RegisterDocumentIds_TheNextDaysRun_DocumentsTheSameExceptForANewVersionOfOneDoc_ReturnsOneRecordToEvaluate()
        {
            _caseEntity.Reset(_transactionId);
            await _caseEntity.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            _caseEntity.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);

            var nextDocs = new CmsDocumentDto[3];
            _cmsDocuments.CopyTo(nextDocs);
            nextDocs[1].VersionId += 1;

            _caseEntity.Reset(_transactionId);

            await _caseEntity.GetCaseDocumentChanges((nextDocs, Array.Empty<PcdRequestDto>(), null));

            using (new AssertionScope())
            {
                _caseEntity.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);
                var newVersion = _caseEntity.CmsDocuments.Find(x => x.CmsDocumentId == nextDocs[1].DocumentId);
                newVersion.VersionId.Should().Be(nextDocs[1].VersionId);
            }
        }

        [Fact]
        public async Task RegisterDocumentIds_TheNextDaysRun_OneDocumentRemovedAndOneANewVersion_ReturnsTwoRecordToEvaluate()
        {
            _caseEntity.Reset(_transactionId);
            await _caseEntity.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            _caseEntity.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);

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

            (CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) newDaysDocumentIdsArg
                = new(newDaysDocuments.ToArray(), _pcdRequests.ToArray(), null);

            _caseEntity.Reset(_transactionId);
            await _caseEntity.GetCaseDocumentChanges(newDaysDocumentIdsArg);

            using (new AssertionScope())
            {
                _caseEntity.CmsDocuments.Count.Should().Be(2);
                var newVersion = _caseEntity.CmsDocuments.Find(x => x.CmsDocumentId == modifiedDocumentId);
                var unmodifiedDocument = _caseEntity.CmsDocuments.Find(x => x.CmsDocumentId == unmodifiedDocumentId);

                newVersion.Should().NotBeNull();
                newVersion?.VersionId.Should().Be(newVersionId);

                unmodifiedDocument.Should().NotBeNull();
                unmodifiedDocument?.VersionId.Should().Be(unmodifiedDocumentVersionId);

                var searchResultForDocumentRemovedFromCms = _caseEntity.CmsDocuments.Find(x => x.CmsDocumentId == documentRemovedFromCmsId);
                searchResultForDocumentRemovedFromCms.Should().BeNull();
            }
        }

        #endregion
    }
}