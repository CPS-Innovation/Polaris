using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using coordinator.Domain.Mapper;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Common.Wrappers.Contracts;
using Common.Wrappers;
using coordinator.Functions.DurableEntity.Entity;
using coordinator.Functions.DurableEntity.Client.Tracker;
using Common.Dto.Tracker;
using Common.Dto.Document;
using Common.Dto.FeatureFlags;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Common.Dto.Case.PreCharge;
using Common.Dto.Case;
using Common.ValueObjects;
using Newtonsoft.Json;

namespace coordinator.tests.Domain.Tracker
{
    public class TrackerTests
    {
        private readonly Fixture _fixture;
        private readonly string _transactionId;
        private readonly List<DocumentDto> _cmsDocuments;
        private readonly List<PcdRequestDto> _pcdRequests;
        private readonly DefendantsAndChargesListDto _defendantsAndChargesList;
        private readonly string _pdfBlobName;
        private readonly (DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) _synchroniseDocumentsArg;
        private readonly List<TrackerCmsDocumentDto> _trackerCmsDocuments;
        private readonly List<TrackerPcdRequestDto> _trackerPcdRequests;
        private readonly string _caseUrn;
        private readonly long _caseId;
        private readonly Guid _correlationId;
        private readonly EntityStateResponse<CaseEntity> _entityStateResponse;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IServiceCollection _services;

        private readonly Mock<IDurableEntityContext> _mockDurableEntityContext;
        private readonly Mock<IDurableEntityClient> _mockDurableEntityClient;
        private readonly Mock<ILogger> _mockLogger;

        private readonly CaseEntity _caseEntity;
        private readonly TrackerClient _trackerStatus;

        public TrackerTests()
        {
            _fixture = new Fixture();
            _transactionId = _fixture.Create<string>();
            _cmsDocuments = _fixture.CreateMany<DocumentDto>(3).ToList();
            _pcdRequests = _fixture.CreateMany<PcdRequestDto>(2).ToList();
            _defendantsAndChargesList = _fixture.Create<DefendantsAndChargesListDto>();
            _correlationId = _fixture.Create<Guid>();
            _trackerCmsDocuments = _fixture.Create<List<TrackerCmsDocumentDto>>();
            _trackerPcdRequests = _fixture.Create<List<TrackerPcdRequestDto>>();
            _caseUrn = _fixture.Create<string>();
            _caseId = _fixture.Create<long>();
            _caseEntity = _fixture.Create<CaseEntity>();
            _pdfBlobName = _fixture.Create<string>();

            _synchroniseDocumentsArg = new (_cmsDocuments.ToArray(), _pcdRequests.ToArray(), _defendantsAndChargesList);
            _entityStateResponse = new EntityStateResponse<CaseEntity>() { EntityExists = true, EntityState=_caseEntity };
            _jsonConvertWrapper = _fixture.Create<JsonConvertWrapper>();
            _services = new ServiceCollection();    

            _mockDurableEntityContext = new Mock<IDurableEntityContext>();
            _mockDurableEntityClient = new Mock<IDurableEntityClient>();
            _mockLogger = new Mock<ILogger>();

            _mockDurableEntityClient
                .Setup
                (
                    client => 
                        client.ReadEntityStateAsync<CaseEntity>
                        (
                            It.Is<EntityId>(e => e.EntityName == nameof(CaseEntity).ToLower() && e.EntityKey == _caseId.ToString()),
                            null, 
                            null
                        )
                )
                .ReturnsAsync(_entityStateResponse);

            _tracker = new TrackerEntity();
            _tracker.TransactionId = _transactionId;
            _tracker.CmsDocuments = _trackerCmsDocuments;
            _tracker.PcdRequests = _trackerPcdRequests;
            _trackerStatus = new TrackerClient(_jsonConvertWrapper);
        }

        [Fact]
        public void Initialise_Initialises()
        {
            _caseEntity.Reset(_transactionId);

            _caseEntity.TransactionId.Should().Be(_transactionId);
            _caseEntity.CmsDocuments.Should().NotBeNull();
            _caseEntity.Status.Should().Be(TrackerStatus.Running);
        }

        [Fact]
        public void Tracker_Serialised_And_Deserialises()
        {
            var serialisedTracker = JsonConvert.SerializeObject(_tracker);
            var deserialisedTracker = JsonConvert.DeserializeObject<TrackerEntity>(serialisedTracker);

            _tracker.CmsDocuments[0].PolarisDocumentId.Should().Be(deserialisedTracker.CmsDocuments[0].PolarisDocumentId);
            _tracker.PcdRequests[0].PolarisDocumentId.Should().Be(deserialisedTracker.PcdRequests[0].PolarisDocumentId);
            deserialisedTracker.TransactionId.Should().Be(_transactionId);
        }

        [Fact]
        public async Task RegisterPdfBlobName_RegistersPdfBlobName()
        {
            _caseEntity.Reset(_transactionId);
            await _caseEntity.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            _caseEntity.RegisterDocumentStatus((_cmsDocuments.First().DocumentId, TrackerDocumentStatus.PdfUploadedToBlob, _pdfBlobName));

            var document = _caseEntity.CmsDocuments.Find(document => document.CmsDocumentId == _cmsDocuments.First().DocumentId);
            document?.PdfBlobName.Should().Be(_pdfBlobName);
            document?.Status.Should().Be(TrackerDocumentStatus.PdfUploadedToBlob);
        }

        [Fact]
        public async Task RegisterDocumentAsAlreadyProcessed_Registers()
        {
            _caseEntity.Reset(_transactionId);
            await _caseEntity.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            _caseEntity.RegisterDocumentStatus((_cmsDocuments.First().DocumentId, TrackerDocumentStatus.DocumentAlreadyProcessed, _pdfBlobName));

            var document = _caseEntity.CmsDocuments.Find(document => document.CmsDocumentId == _cmsDocuments.First().DocumentId);
            document?.Status.Should().Be(TrackerDocumentStatus.DocumentAlreadyProcessed);
        }

        [Fact]
        public async Task RegisterDocumentAsFailedPDFConversion_Registers()
        {
            _caseEntity.Reset(_transactionId);
            await _caseEntity.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            _caseEntity.RegisterDocumentStatus((_cmsDocuments.First().DocumentId, TrackerDocumentStatus.UnableToConvertToPdf, null));

            var document = _caseEntity.CmsDocuments.Find(document => document.CmsDocumentId == _cmsDocuments.First().DocumentId);
            document?.Status.Should().Be(TrackerDocumentStatus.UnableToConvertToPdf);
        }

        [Fact]
        public async Task Initialisation_SetsDocumentStatusToNone()
        {
            _caseEntity.Reset(_transactionId);
            await _caseEntity.GetCaseDocumentChanges(_synchroniseDocumentsArg);

            var document = _caseEntity.CmsDocuments.Find(document => document.CmsDocumentId == _cmsDocuments.First().DocumentId);
            document?.Status.Should().Be(TrackerDocumentStatus.New);
        }

        [Fact]
        public async Task RegisterUnexpectedDocumentFailure_Registers()
        {
            _caseEntity.Reset(_transactionId);
            await _caseEntity.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            _caseEntity.RegisterDocumentStatus((_cmsDocuments.First().DocumentId, TrackerDocumentStatus.UnexpectedFailure, null));

            var document = _caseEntity.CmsDocuments.Find(document => document.CmsDocumentId == _cmsDocuments.First().DocumentId);
            document?.Status.Should().Be(TrackerDocumentStatus.UnexpectedFailure);
        }

        [Fact]
        public async Task RegisterIndexed_RegistersIndexed()
        {
            _caseEntity.Reset(_transactionId);
            await _caseEntity.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            _caseEntity.RegisterDocumentStatus((_cmsDocuments.First().DocumentId, TrackerDocumentStatus.Indexed, null));

            var document = _caseEntity.CmsDocuments.Find(document => document.CmsDocumentId == _cmsDocuments.First().DocumentId);
            document?.Status.Should().Be(TrackerDocumentStatus.Indexed);
        }

        [Fact]
        public async Task RegisterIndexed_RegistersOcrAndIndexFailure()
        {
            _caseEntity.Reset(_transactionId);
            await _caseEntity.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            _caseEntity.RegisterDocumentStatus((_cmsDocuments.First().DocumentId, TrackerDocumentStatus.OcrAndIndexFailure, null));

            var document = _caseEntity.CmsDocuments.Find(document => document.CmsDocumentId == _cmsDocuments.First().DocumentId);
            document?.Status.Should().Be(TrackerDocumentStatus.OcrAndIndexFailure);
        }

        [Fact]
        public void RegisterCompleted_RegistersCompleted()
        {
            _caseEntity.Reset(_transactionId);
            _caseEntity.RegisterCompleted((It.IsAny<DateTime>(), true));

            _caseEntity.Status.Should().Be(TrackerStatus.Completed);
            _caseEntity.ProcessingCompleted.Should().NotBeNull();
        }

        [Fact]
        public void RegisterFailed_RegistersFailed()
        {
            _caseEntity.Reset(_transactionId);
            _caseEntity.RegisterCompleted((It.IsAny<DateTime>(), false));

            _caseEntity.Status.Should().Be(TrackerStatus.Failed);
        }

        //[Fact]
        //public async Task GetDocuments_ReturnsDocuments()
        //{
        //    _tracker.CmsDocuments = _trackerDocuments;
        //    var documents = await _tracker.GetDocuments();

        //    documents.Should().BeEquivalentTo(_trackerDocuments);
        //}

        [Fact]
        public async Task AllDocumentsFailed_ReturnsTrueIfAllDocumentsFailed()
        {
            _tracker.CmsDocuments = new List<TrackerCmsDocumentDto> {
                new(_fixture.Create<PolarisDocumentId>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(), _fixture.Create<string>(), true, _fixture.Create<PresentationFlagsDto>()) { Status = TrackerDocumentStatus.UnableToConvertToPdf},
                new(_fixture.Create<PolarisDocumentId>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(),  _fixture.Create<string>(), true, _fixture.Create<PresentationFlagsDto>()) { Status = TrackerDocumentStatus.UnexpectedFailure}
            };
            _caseEntity.PcdRequests = new List<TrackerPcdRequestDto>();
            _caseEntity.DefendantsAndCharges = new TrackerDefendantsAndChargesDto { Status = TrackerDocumentStatus.UnableToConvertToPdf };

            var output = await _caseEntity.AllDocumentsFailed();

            output.Should().BeTrue();
        }

        [Fact]
        public async Task AllDocumentsFailed_ReturnsFalseIfAllDocumentsHaveNotFailed()
        {
            _tracker.CmsDocuments = new List<TrackerCmsDocumentDto> {
                new(_fixture.Create<PolarisDocumentId>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(), _fixture.Create<string>(), true, _fixture.Create<PresentationFlagsDto>()) { Status = TrackerDocumentStatus.UnableToConvertToPdf},
                new(_fixture.Create<PolarisDocumentId>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(), _fixture.Create<string>(), true, _fixture.Create<PresentationFlagsDto>()) { Status = TrackerDocumentStatus.UnexpectedFailure},
                new(_fixture.Create<PolarisDocumentId>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(), _fixture.Create<string>(), true, _fixture.Create<PresentationFlagsDto>()) { Status = TrackerDocumentStatus.PdfUploadedToBlob},
            };
            _caseEntity.PcdRequests = new List<TrackerPcdRequestDto>();
            _caseEntity.DefendantsAndCharges = new TrackerDefendantsAndChargesDto { Status = TrackerDocumentStatus.Indexed };

            var output = await _caseEntity.AllDocumentsFailed();

            output.Should().BeFalse();
        }

        [Fact]
        public async Task Run_Tracker_Dispatches()
        {
            await CaseEntity.Run(_mockDurableEntityContext.Object);

            _mockDurableEntityContext.Verify(context => context.DispatchAsync<CaseEntity>());
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsOK()
        {
            var message = new HttpRequestMessage();
            message.Headers.Add("Correlation-Id", _correlationId.ToString());
            var response = await _trackerStatus.HttpStart(message, _caseUrn, _caseId.ToString(), _mockDurableEntityClient.Object, _mockLogger.Object);

            response.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsEntityState()
        {
            var message = new HttpRequestMessage();
            message.Headers.Add("Correlation-Id", _correlationId.ToString());
            var response = await _trackerStatus.HttpStart(message, _caseUrn, _caseId.ToString(), _mockDurableEntityClient.Object, _mockLogger.Object);

            var okObjectResult = response as OkObjectResult;

            okObjectResult?.Value.Should().Be(_entityStateResponse.EntityState);
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsNotFoundIfEntityNotFound()
        {
            var entityStateResponse = new EntityStateResponse<CaseEntity>() { EntityExists = false };
            _mockDurableEntityClient.Setup(
                client => client.ReadEntityStateAsync<CaseEntity>(
                    It.Is<EntityId>(e => e.EntityName == nameof(CaseEntity).ToLower() && e.EntityKey == _caseId.ToString()),
                    null, null))
                .ReturnsAsync(entityStateResponse);

            var message = new HttpRequestMessage();
            message.Headers.Add("Correlation-Id", _correlationId.ToString());
            var response = await _trackerStatus.HttpStart(message, _caseUrn, _caseId.ToString(), _mockDurableEntityClient.Object, _mockLogger.Object);

            response.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsBadRequestIfCorrelationIdNotFound()
        {
            var entityStateResponse = new EntityStateResponse<CaseEntity>() { EntityExists = false };
            _mockDurableEntityClient.Setup(
                    client => client.ReadEntityStateAsync<CaseEntity>(
                        It.Is<EntityId>(e => e.EntityName == nameof(CaseEntity).ToLower() && e.EntityKey == _caseId.ToString()),
                        null, null))
                .ReturnsAsync(entityStateResponse);

            var message = new HttpRequestMessage();
            var response = await _trackerStatus.HttpStart(message, _caseUrn, _caseId.ToString(), _mockDurableEntityClient.Object, _mockLogger.Object);

            response.Should().BeOfType<BadRequestObjectResult>();
        }

        #region SynchroniseDocument

        [Fact]
        public async Task SynchroniseDocument_CreatesNewDocumentsAndPcdRequests()
        {
            // Arrange
            CaseEntity tracker = new CaseEntity();
            tracker.Reset(_transactionId);

            // Act
            var deltas = await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);

            // Assert
            (await tracker.GetVersion()).Should().Be(1);
            tracker.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);
            deltas.CreatedCmsDocuments.Count.Should().Be(_cmsDocuments.Count);
            deltas.UpdatedCmsDocuments.Count.Should().Be(0);
            deltas.DeletedCmsDocuments.Count.Should().Be(0);
            deltas.CreatedPcdRequests.Count.Should().Be(_pcdRequests.Count);
            deltas.UpdatedPcdRequests.Count.Should().Be(0);
            deltas.DeletedPcdRequests.Count.Should().Be(0);
            deltas.Any().Should().BeTrue();
            tracker.CmsDocuments[0].PolarisDocumentVersionId.Should().Be(1);
            tracker.CmsDocuments[1].PolarisDocumentVersionId.Should().Be(1);
            tracker.CmsDocuments[2].PolarisDocumentVersionId.Should().Be(1);
            tracker.PcdRequests[0].PolarisDocumentVersionId.Should().Be(1);
            tracker.PcdRequests[1].PolarisDocumentVersionId.Should().Be(1);
        }

        [Fact]
        public async Task SynchroniseDocument_NoChangesWithExistingDocumentAndVersionIds()
        {
            // Arrange
            CaseEntity tracker = new CaseEntity();
            tracker.Reset(_transactionId);
            await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            tracker.CmsDocuments.ForEach(doc => doc.Status = TrackerDocumentStatus.Indexed);
            tracker.PcdRequests.ForEach(pcd => pcd.Status = TrackerDocumentStatus.Indexed);

            // Act 
            var deltas = await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);

            // Assert
            (await tracker.GetVersion()).Should().Be(1);
            tracker.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);
            deltas.CreatedCmsDocuments.Count.Should().Be(0);
            deltas.UpdatedCmsDocuments.Count.Should().Be(0);
            deltas.DeletedCmsDocuments.Count.Should().Be(0);
            deltas.Any().Should().BeFalse();
            tracker.CmsDocuments[0].PolarisDocumentVersionId.Should().Be(1);
            tracker.CmsDocuments[1].PolarisDocumentVersionId.Should().Be(1);
            tracker.CmsDocuments[2].PolarisDocumentVersionId.Should().Be(1);
        }

        [Fact]
        public async Task SynchroniseDocument_IndexedCmsDocumentAreRetained()
        {
            // Arrange
            CaseEntity tracker = new CaseEntity();
            tracker.Reset(_transactionId);
            await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            tracker.CmsDocuments.ForEach(doc => doc.Status = TrackerDocumentStatus.Indexed);
            tracker.PcdRequests.ForEach(pcd => pcd.Status = TrackerDocumentStatus.Indexed);

            // Act 
            var deltas = await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);

            // Assert
            (await tracker.GetVersion()).Should().Be(1);
            tracker.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);
            deltas.CreatedCmsDocuments.Count.Should().Be(0);
            deltas.UpdatedCmsDocuments.Count.Should().Be(0);
            deltas.DeletedCmsDocuments.Count.Should().Be(0);
            deltas.Any().Should().BeFalse();
            tracker.CmsDocuments[0].PolarisDocumentVersionId.Should().Be(1);
            tracker.CmsDocuments[1].PolarisDocumentVersionId.Should().Be(1);
            tracker.CmsDocuments[2].PolarisDocumentVersionId.Should().Be(1);
        }

        [Fact]
        public async Task SynchroniseDocument_FailedCmsDocumentAreReprocessed()
        {
            // Arrange
            CaseEntity tracker = new CaseEntity();
            tracker.Reset(_transactionId);
            await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            tracker.CmsDocuments.ForEach(doc => doc.Status = TrackerDocumentStatus.UnexpectedFailure);
            tracker.PcdRequests.ForEach(pcd => pcd.Status = TrackerDocumentStatus.Indexed);

            // Act 
            var deltas = await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);

            // Assert
            (await tracker.GetVersion()).Should().Be(2);
            tracker.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);
            deltas.CreatedCmsDocuments.Count.Should().Be(0);
            deltas.UpdatedCmsDocuments.Count.Should().Be(3);
            deltas.DeletedCmsDocuments.Count.Should().Be(0);
            deltas.Any().Should().BeTrue();
            tracker.CmsDocuments[0].PolarisDocumentVersionId.Should().Be(2);
            tracker.CmsDocuments[1].PolarisDocumentVersionId.Should().Be(2);
            tracker.CmsDocuments[2].PolarisDocumentVersionId.Should().Be(2);
        }

        [Fact]
        public async Task SynchroniseDocument_FailedPcdRequestsAreReprocessed()
        {
            // Arrange
            CaseEntity tracker = new CaseEntity();
            tracker.Reset(_transactionId);
            await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            tracker.CmsDocuments.ForEach(doc => doc.Status = TrackerDocumentStatus.Indexed);
            tracker.PcdRequests.ForEach(pcd => pcd.Status = TrackerDocumentStatus.UnexpectedFailure);

            // Act 
            var deltas = await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);

            // Assert
            (await tracker.GetVersion()).Should().Be(2);
            tracker.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);
            deltas.CreatedCmsDocuments.Count.Should().Be(0);
            deltas.UpdatedCmsDocuments.Count.Should().Be(0);
            deltas.DeletedCmsDocuments.Count.Should().Be(0);
            deltas.CreatedPcdRequests.Count.Should().Be(0);
            deltas.UpdatedPcdRequests.Count.Should().Be(2);
            deltas.DeletedPcdRequests.Count.Should().Be(0);
            deltas.Any().Should().BeTrue();
            tracker.PcdRequests[0].PolarisDocumentVersionId.Should().Be(2);
            tracker.PcdRequests[1].PolarisDocumentVersionId.Should().Be(2);
        }

        [Fact]
        public async Task SynchroniseDocument_ChangesWithUpdatedDocumentAndVersionIds()
        {
            // Arrange
            CaseEntity tracker = new CaseEntity();
            tracker.Reset(_transactionId);
            await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            tracker.CmsDocuments.ForEach(doc => doc.Status = TrackerDocumentStatus.Indexed);
            tracker.PcdRequests.ForEach(pcd => pcd.Status = TrackerDocumentStatus.Indexed);

            // Act 
            _synchroniseDocumentsArg.CmsDocuments[1].VersionId = 111111111;
            _synchroniseDocumentsArg.CmsDocuments[2].VersionId = 222222222;
            var deltas = await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);

            // Assert
            (await tracker.GetVersion()).Should().Be(2);
            tracker.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);
            deltas.CreatedCmsDocuments.Count.Should().Be(0);
            deltas.UpdatedCmsDocuments.Count.Should().Be(2);
            deltas.DeletedCmsDocuments.Count.Should().Be(0);
            deltas.Any().Should().BeTrue();
            tracker.CmsDocuments[0].PolarisDocumentVersionId.Should().Be(1);
            tracker.CmsDocuments[1].PolarisDocumentVersionId.Should().Be(2);
            tracker.CmsDocuments[2].PolarisDocumentVersionId.Should().Be(2);
        }

        [Fact]
        public async Task SynchroniseDocument_ChangesWithDeletedDocuments()
        {
            // Arrange
            CaseEntity tracker = new CaseEntity();
            tracker.Reset(_transactionId);
            await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            tracker.CmsDocuments.ForEach(doc => doc.Status = TrackerDocumentStatus.Indexed);
            tracker.PcdRequests.ForEach(doc => doc.Status = TrackerDocumentStatus.Indexed);
            (DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) synchroniseDocumentsArg
                = new(_cmsDocuments.Take(1).ToArray(), _pcdRequests.Take(1).ToArray(), null);

            // Act 
            var deltas = await tracker.GetCaseDocumentChanges(synchroniseDocumentsArg);

            // Assert
            (await tracker.GetVersion()).Should().Be(2);
            tracker.CmsDocuments.Count.Should().Be(1);
            deltas.CreatedCmsDocuments.Count.Should().Be(0);
            deltas.UpdatedCmsDocuments.Count.Should().Be(0);
            deltas.DeletedCmsDocuments.Count.Should().Be(2);
            deltas.CreatedPcdRequests.Count.Should().Be(0);
            deltas.UpdatedPcdRequests.Count.Should().Be(0);
            deltas.DeletedPcdRequests.Count.Should().Be(1);
            deltas.Any().Should().BeTrue();
            tracker.CmsDocuments[0].PolarisDocumentVersionId.Should().Be(1);
            tracker.PcdRequests[0].PolarisDocumentVersionId.Should().Be(1);
        }

        [Fact]
        public async Task RegisterDocumentIds_TheNextDaysRun_DocumentsTheSame_ReturnsNothingToEvaluate()
        {
            _caseEntity.Reset(_transactionId);
            await _caseEntity.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            _caseEntity.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);

            var newDaysDocuments = new DocumentDto[3];
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

            var newDaysDocuments = new List<DocumentDto> { _cmsDocuments.First() };
            ////only one document in today's run, the next two should be removed from the tracker and in the evaluation results

            (DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) newDaysDocumentIdsArg =
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

            var newDaysDocuments = new DocumentDto[3];
            _cmsDocuments.CopyTo(newDaysDocuments);
            var originalVersionId = newDaysDocuments[1].VersionId;
            var newVersionId = originalVersionId + 1;
            newDaysDocuments[1].VersionId = newVersionId;
            var modifiedDocumentId = newDaysDocuments[1].DocumentId;
            (DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) newDaysDocumentIdsArg =
                new(newDaysDocuments.ToArray(), Array.Empty<PcdRequestDto>(), null);

            _caseEntity.Reset(_transactionId);
            await _caseEntity.GetCaseDocumentChanges(newDaysDocumentIdsArg);

            using (new AssertionScope())
            {
                _caseEntity.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);
                var newVersion = _caseEntity.CmsDocuments.Find(x => x.CmsDocumentId == modifiedDocumentId);

                newVersion.Should().NotBeNull();
                newVersion?.CmsVersionId.Should().Be(newVersionId);
            }
        }

        [Fact]
        public async Task RegisterDocumentIds_TheNextDaysRun_OneDocumentRemovedAndOneANewVersion_ReturnsTwoRecordToEvaluate()
        {
            _caseEntity.Reset(_transactionId);
            await _caseEntity.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            _caseEntity.CmsDocuments.Count.Should().Be(_cmsDocuments.Count);

            var newDaysDocuments = new List<DocumentDto>
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

            (DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) newDaysDocumentIdsArg
                = new(newDaysDocuments.ToArray(), _pcdRequests.ToArray(), null);

            _caseEntity.Reset(_transactionId);
            await _caseEntity.GetCaseDocumentChanges(newDaysDocumentIdsArg);

            using (new AssertionScope())
            {
                _caseEntity.CmsDocuments.Count.Should().Be(2);
                var newVersion = _caseEntity.CmsDocuments.Find(x => x.CmsDocumentId == modifiedDocumentId);
                var unmodifiedDocument = _caseEntity.CmsDocuments.Find(x => x.CmsDocumentId == unmodifiedDocumentId);

                newVersion.Should().NotBeNull();
                newVersion?.CmsVersionId.Should().Be(newVersionId);

                unmodifiedDocument.Should().NotBeNull();
                unmodifiedDocument?.CmsVersionId.Should().Be(unmodifiedDocumentVersionId);

                var searchResultForDocumentRemovedFromCms = _caseEntity.CmsDocuments.Find(x => x.CmsDocumentId == documentRemovedFromCmsId);
                searchResultForDocumentRemovedFromCms.Should().BeNull();
            }
        }

        #endregion

        [Fact]
        public void TrackerEntityMapsToTrackerDto()
        {
            // Arrange
            _services.RegisterMapsterConfiguration();
            var caseEntity = _fixture.Create<CaseEntity>();
            var caseRefreshLogsEntity = _fixture.Create<CaseRefreshLogsEntity>();
        
            // Act
            var trackerDto = (caseEntity, caseRefreshLogsEntity).Adapt<TrackerDto>();

            // Assert
            trackerDto.Documents.Count.Should().Be(caseEntity.CmsDocuments.Count + caseEntity.PcdRequests.Count + 1);
        }
    }
}
