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
using Microsoft.Extensions.DependencyInjection;
using Common.Dto.Case.PreCharge;
using Common.Dto.Case;
using Common.ValueObjects;
using Newtonsoft.Json;
using Common.Domain.Entity;
using coordinator.Functions.DurableEntity.Entity.Mapper;

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
        private readonly long _caseId;
        private readonly Guid _correlationId;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IServiceCollection _services;

        private readonly Mock<IDurableEntityContext> _mockDurableEntityContext;
        private readonly Mock<IDurableEntityClient> _mockDurableEntityClient;
        private readonly Mock<ILogger> _mockLogger;

        private readonly CaseDurableEntity _caseEntity;
        private readonly EntityStateResponse<CaseDurableEntity> _entityStateResponse;
        private readonly CaseRefreshLogsDurableEntity _caseRefreshLogsEntity;
        private readonly EntityStateResponse<CaseRefreshLogsDurableEntity> _caseRefreshLogsEntityStateResponse;
        private readonly TrackerClient _trackerStatus;

        public TrackerTests()
        {
            _fixture = new Fixture();
            _transactionId = _fixture.Create<string>();
            _cmsDocuments = _fixture.CreateMany<CmsDocumentDto>(3).ToList();
            _pcdRequests = _fixture.CreateMany<PcdRequestDto>(2).ToList();
            _defendantsAndChargesList = _fixture.Create<DefendantsAndChargesListDto>();
            _correlationId = _fixture.Create<Guid>();
            _trackerCmsDocuments = _fixture.Create<List<CmsDocumentEntity>>();
            _trackerPcdRequests = _fixture.Create<List<PcdRequestEntity>>();
            _caseUrn = _fixture.Create<string>();
            _caseId = _fixture.Create<long>();
            _caseEntity = _fixture.Create<CaseDurableEntity>();
            _caseRefreshLogsEntity = _fixture.Create<CaseRefreshLogsDurableEntity>();

            _pdfBlobName = _fixture.Create<string>();

            _synchroniseDocumentsArg = new (_cmsDocuments.ToArray(), _pcdRequests.ToArray(), _defendantsAndChargesList);
            _entityStateResponse = new EntityStateResponse<CaseDurableEntity>() { EntityExists = true, EntityState=_caseEntity };
            _caseRefreshLogsEntityStateResponse = new EntityStateResponse<CaseRefreshLogsDurableEntity>() { EntityExists = true, EntityState = _caseRefreshLogsEntity };
            _jsonConvertWrapper = _fixture.Create<JsonConvertWrapper>();
            _services = new ServiceCollection();    

            _mockDurableEntityContext = new Mock<IDurableEntityContext>();
            _mockDurableEntityClient = new Mock<IDurableEntityClient>();
            _mockLogger = new Mock<ILogger>();

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

            _mockDurableEntityClient
                .Setup
                (
                    client =>
                        client.ReadEntityStateAsync<CaseRefreshLogsDurableEntity>
                        (
                            It.Is<EntityId>(e => e.EntityName == nameof(CaseRefreshLogsDurableEntity).ToLower() && e.EntityKey.StartsWith(_caseId.ToString())),
                            null,
                            null
                        )
                )
                .ReturnsAsync(_caseRefreshLogsEntityStateResponse);

            _caseEntity = new CaseDurableEntity();
            _caseEntity.TransactionId = _transactionId;
            _caseEntity.CmsDocuments = _trackerCmsDocuments;
            _caseEntity.PcdRequests = _trackerPcdRequests;
            _trackerStatus = new TrackerClient(_jsonConvertWrapper);
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

            _caseEntity.CmsDocuments[0].PolarisDocumentId.Should().Be(deserialisedTracker.CmsDocuments[0].PolarisDocumentId);
            _caseEntity.PcdRequests[0].PolarisDocumentId.Should().Be(deserialisedTracker.PcdRequests[0].PolarisDocumentId);
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

        [Theory]
        [InlineData(DocumentStatus.Indexed)]
        [InlineData(DocumentStatus.DocumentAlreadyProcessed)]
        [InlineData(DocumentStatus.UnableToConvertToPdf)]
        [InlineData(DocumentStatus.PdfUploadedToBlob)]
        [InlineData(DocumentStatus.OcrAndIndexFailure)]
        public async Task RegisterIndexed_RegistersStates(DocumentStatus status)
        {
            // Arrange
            _caseEntity.Reset(_transactionId);
            await _caseEntity.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            var polarisDocumentId = _caseEntity.CmsDocuments.First().PolarisDocumentIdValue;
            var cmsDocumentId = _caseEntity.CmsDocuments.First().CmsDocumentId;

            // Act
            _caseEntity.SetDocumentStatus((polarisDocumentId, status, _pdfBlobName));

            // Assert
            var document = _caseEntity.CmsDocuments.Find(document => document.CmsDocumentId == cmsDocumentId);
            document?.Status.Should().Be(status);
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
            _caseEntity.CmsDocuments = new List<CmsDocumentEntity> {
                new(_fixture.Create<PolarisDocumentId>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), true, _fixture.Create<int?>(), _fixture.Create<PresentationFlagsDto>()) { Status = DocumentStatus.UnableToConvertToPdf},
                new(_fixture.Create<PolarisDocumentId>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(),  _fixture.Create<string>(), true, _fixture.Create<int?>(), _fixture.Create<PresentationFlagsDto>()) { Status = DocumentStatus.UnableToConvertToPdf}
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
                new(_fixture.Create<PolarisDocumentId>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), true, _fixture.Create<int?>(), _fixture.Create<PresentationFlagsDto>()) { Status = DocumentStatus.UnableToConvertToPdf},
                new(_fixture.Create<PolarisDocumentId>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), true, _fixture.Create<int?>(), _fixture.Create<PresentationFlagsDto>()) { Status = DocumentStatus.UnableToConvertToPdf},
                new(_fixture.Create<PolarisDocumentId>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), true, _fixture.Create<int?>(), _fixture.Create<PresentationFlagsDto>()) { Status = DocumentStatus.PdfUploadedToBlob},
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
            var message = new HttpRequestMessage();
            message.Headers.Add("Correlation-Id", _correlationId.ToString());

            // Act
            var response = await _trackerStatus.HttpStart(message, _caseUrn, _caseId.ToString(), _mockDurableEntityClient.Object, _mockLogger.Object);

            // Assert
            response.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsTrackerDto()
        {
            var message = new HttpRequestMessage();
            message.Headers.Add("Correlation-Id", _correlationId.ToString());
            var response = await _trackerStatus.HttpStart(message, _caseUrn, _caseId.ToString(), _mockDurableEntityClient.Object, _mockLogger.Object);

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

            var message = new HttpRequestMessage();
            message.Headers.Add("Correlation-Id", _correlationId.ToString());
            var response = await _trackerStatus.HttpStart(message, _caseUrn, _caseId.ToString(), _mockDurableEntityClient.Object, _mockLogger.Object);

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

            var message = new HttpRequestMessage();
            var response = await _trackerStatus.HttpStart(message, _caseUrn, _caseId.ToString(), _mockDurableEntityClient.Object, _mockLogger.Object);

            response.Should().BeOfType<BadRequestObjectResult>();
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
            tracker.CmsDocuments[0].PolarisDocumentVersionId.Should().Be(1);
            tracker.CmsDocuments[1].PolarisDocumentVersionId.Should().Be(1);
            tracker.CmsDocuments[2].PolarisDocumentVersionId.Should().Be(1);
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
            tracker.CmsDocuments[0].PolarisDocumentVersionId.Should().Be(1);
            tracker.CmsDocuments[1].PolarisDocumentVersionId.Should().Be(1);
            tracker.CmsDocuments[2].PolarisDocumentVersionId.Should().Be(1);
        }

        [Fact]
        public async Task SynchroniseDocument_FailedCmsDocumentAreReprocessed()
        {
            // Arrange
            CaseDurableEntity tracker = new CaseDurableEntity();
            tracker.Reset(_transactionId);
            await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            tracker.CmsDocuments.ForEach(doc => doc.Status = DocumentStatus.UnableToConvertToPdf);
            tracker.PcdRequests.ForEach(pcd => pcd.Status = DocumentStatus.Indexed);

            // Act 
            var deltas = await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);

            // Assert
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
            CaseDurableEntity tracker = new CaseDurableEntity();
            tracker.Reset(_transactionId);
            await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);
            tracker.CmsDocuments.ForEach(doc => doc.Status = DocumentStatus.Indexed);
            tracker.PcdRequests.ForEach(pcd => pcd.Status = DocumentStatus.UnableToConvertToPdf);

            // Act 
            var deltas = await tracker.GetCaseDocumentChanges(_synchroniseDocumentsArg);

            // Assert
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
            tracker.CmsDocuments[0].PolarisDocumentVersionId.Should().Be(1);
            tracker.CmsDocuments[1].PolarisDocumentVersionId.Should().Be(2);
            tracker.CmsDocuments[2].PolarisDocumentVersionId.Should().Be(2);
        }

        [Fact]
        public async Task SynchroniseDocument_ChangesWithDeletedDocuments()
        {
            // Arrange
            CaseDurableEntity tracker = new CaseDurableEntity();
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
            tracker.CmsDocuments[0].PolarisDocumentVersionId.Should().Be(1);
            tracker.PcdRequests[0].PolarisDocumentVersionId.Should().Be(1);
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

            var newDaysDocuments = new CmsDocumentDto[3];
            _cmsDocuments.CopyTo(newDaysDocuments);
            var originalVersionId = newDaysDocuments[1].VersionId;
            var newVersionId = originalVersionId + 1;
            newDaysDocuments[1].VersionId = newVersionId;
            var modifiedDocumentId = newDaysDocuments[1].DocumentId;
            (CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) newDaysDocumentIdsArg =
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
                newVersion?.CmsVersionId.Should().Be(newVersionId);

                unmodifiedDocument.Should().NotBeNull();
                unmodifiedDocument?.CmsVersionId.Should().Be(unmodifiedDocumentVersionId);

                var searchResultForDocumentRemovedFromCms = _caseEntity.CmsDocuments.Find(x => x.CmsDocumentId == documentRemovedFromCmsId);
                searchResultForDocumentRemovedFromCms.Should().BeNull();
            }
        }

        #endregion

        [Fact]
        public void Populated_TrackerEntity_MapsTo_TrackerDto()
        {
            // Arrange
            _services.RegisterMapsterConfiguration();
            var caseEntity = _fixture.Create<CaseDurableEntity>();
            caseEntity.CmsDocuments[0].CategoryListOrder = 1;
            var caseRefreshLogsEntity = _fixture.Create<CaseRefreshLogsDurableEntity>();


            // Act
            var trackerDto = CaseDurableEntityMapper.MapCase(caseEntity, caseRefreshLogsEntity);


            // Assert
            trackerDto.Documents.Count.Should().Be(caseEntity.CmsDocuments.Count + caseEntity.PcdRequests.Count + 1);
            trackerDto.Documents.Count(d => d.CategoryListOrder != null).Should().BeGreaterThan(0);
        }

        [Fact]
        public void Empty_TrackerEntity_MapsTo_TrackerDto()
        {
            // Arrange
            _services.RegisterMapsterConfiguration();
            var caseEntity = new CaseDurableEntity();
            var caseRefreshLogsEntity = new CaseRefreshLogsDurableEntity();


            // Act
            var trackerDto = CaseDurableEntityMapper.MapCase(caseEntity, caseRefreshLogsEntity);

            // Assert
            trackerDto.Documents.Count.Should().Be(0);
            trackerDto.Logs.Case.Count.Should().Be(0);
            trackerDto.Logs.Documents.Count.Should().Be(0);
            trackerDto.Status.Should().Be(CaseRefreshStatus.NotStarted);
        }

        [Fact]
        public void Null_TrackerEntity_MapsTo_TrackerDto()
        {
            // Arrange
            _services.RegisterMapsterConfiguration();
            CaseDurableEntity caseEntity = null;
            CaseRefreshLogsDurableEntity caseRefreshLogsEntity = null;


            // Act
            var trackerDto = CaseDurableEntityMapper.MapCase(caseEntity, caseRefreshLogsEntity);

            // Assert
            trackerDto.Documents.Count.Should().Be(0);
            trackerDto.Logs.Case.Count.Should().Be(0);
            trackerDto.Logs.Documents.Count.Should().Be(0);
            trackerDto.Status.Should().Be(CaseRefreshStatus.NotStarted);
        }
    }
}