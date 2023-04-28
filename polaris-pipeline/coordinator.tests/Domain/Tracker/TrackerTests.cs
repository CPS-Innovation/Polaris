using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using coordinator.Domain.Tracker;
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

namespace coordinator.tests.Domain.Tracker
{
    public class TrackerTests
    {
        private readonly Fixture _fixture;
        private readonly string _transactionId;
        private readonly List<DocumentDto> _documents;
        private readonly RegisterPdfBlobNameArg _pdfBlobNameArg;
        //private readonly SynchroniseDocumentsArg _synchroniseDocumentsArg;
        private readonly List<TrackerCmsDocumentDto> _trackerDocuments;
        private readonly string _caseUrn;
        private readonly long _caseId;
        private readonly Guid _correlationId;
        private readonly EntityStateResponse<TrackerEntity> _entityStateResponse;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IServiceCollection _services;

        private readonly Mock<IDurableEntityContext> _mockDurableEntityContext;
        private readonly Mock<IDurableEntityClient> _mockDurableEntityClient;
        private readonly Mock<ILogger> _mockLogger;

        private readonly TrackerEntity _tracker;
        private readonly TrackerClient _trackerStatus;

        public TrackerTests()
        {
            _fixture = new Fixture();
            _transactionId = _fixture.Create<string>();
            _documents = _fixture.CreateMany<DocumentDto>(3).ToList();
            _correlationId = _fixture.Create<Guid>();
            _pdfBlobNameArg = _fixture.Build<RegisterPdfBlobNameArg>()
                                .With(a => a.DocumentId, _documents.First().DocumentId)
                                .With(a => a.VersionId, _documents.First().VersionId)
                                .Create();
            _trackerDocuments = _fixture.Create<List<TrackerCmsDocumentDto>>();
            _caseUrn = _fixture.Create<string>();
            _caseId = _fixture.Create<long>();
            _synchroniseDocumentsArg = _fixture.Build<SynchroniseDocumentsArg>()
                .With(a => a.CaseUrn, _caseUrn)
                .With(a => a.CaseId, _caseId)
                .With(a => a.Documents, _documents)
                .With(a => a.CorrelationId, _correlationId)
                .Create();
            _entityStateResponse = new EntityStateResponse<TrackerEntity>() { EntityExists = true };
            _jsonConvertWrapper = _fixture.Create<JsonConvertWrapper>();
            _services = new ServiceCollection();    

            _mockDurableEntityContext = new Mock<IDurableEntityContext>();
            _mockDurableEntityClient = new Mock<IDurableEntityClient>();
            _mockLogger = new Mock<ILogger>();

            _mockDurableEntityClient.Setup(
                client => client.ReadEntityStateAsync<TrackerEntity>(
                    It.Is<EntityId>(e => e.EntityName == nameof(TrackerEntity).ToLower() && e.EntityKey == _caseId.ToString()),
                    null, null))
                .ReturnsAsync(_entityStateResponse);

            _tracker = new TrackerEntity();
            _trackerStatus = new TrackerClient(_jsonConvertWrapper);
        }

        [Fact]
        public async Task Initialise_Initialises()
        {
            await _tracker.Reset((It.IsAny<DateTime>(), _transactionId));

            _tracker.TransactionId.Should().Be(_transactionId);
            _tracker.CmsDocuments.Should().NotBeNull();
            _tracker.Logs.Should().NotBeNull();
            _tracker.Status.Should().Be(TrackerStatus.Running);

            _tracker.Logs.Count.Should().Be(1);
        }

        [Fact]
        public async Task RegisterPdfBlobName_RegistersPdfBlobName()
        {
            await _tracker.Reset((It.IsAny<DateTime>(), _transactionId));
            await _tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            await _tracker.RegisterPdfBlobName(_pdfBlobNameArg);

            var document = _tracker.CmsDocuments.Find(document => document.CmsDocumentId == _documents.First().DocumentId);
            document?.PdfBlobName.Should().Be(_pdfBlobNameArg.BlobName);
            document?.Status.Should().Be(TrackerDocumentStatus.PdfUploadedToBlob);

            _tracker.Logs.Count.Should().Be(10);
        }

        [Fact]
        public async Task RegisterDocumentAsAlreadyProcessed_Registers()
        {
            await _tracker.Reset((It.IsAny<DateTime>(), _transactionId));
            await _tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            await _tracker.RegisterBlobAlreadyProcessed(new RegisterPdfBlobNameArg(It.IsAny<DateTime>(), _pdfBlobNameArg.DocumentId, _pdfBlobNameArg.VersionId, _pdfBlobNameArg.BlobName));

            var document = _tracker.CmsDocuments.Find(document => document.CmsDocumentId == _pdfBlobNameArg.DocumentId);
            document?.Status.Should().Be(TrackerDocumentStatus.DocumentAlreadyProcessed);

            _tracker.Logs.Count.Should().Be(10);
        }

        [Fact]
        public async Task RegisterDocumentAsFailedPDFConversion_Registers()
        {
            await _tracker.Reset((It.IsAny<DateTime>(), _transactionId));
            await _tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            await _tracker.RegisterStatus((It.IsAny<DateTime>(), _pdfBlobNameArg.DocumentId, TrackerDocumentStatus.UnableToConvertToPdf, TrackerLogType.UnableToConvertDocumentToPdf));

            var document = _tracker.CmsDocuments.Find(document => document.CmsDocumentId == _pdfBlobNameArg.DocumentId);
            document?.Status.Should().Be(TrackerDocumentStatus.UnableToConvertToPdf);

            _tracker.Logs.Count.Should().Be(10);
        }

        [Fact]
        public async Task Initialisation_SetsDocumentStatusToNone()
        {
            await _tracker.Reset((It.IsAny<DateTime>(), _transactionId));
            await _tracker.SynchroniseDocuments(_synchroniseDocumentsArg);

            var document = _tracker.CmsDocuments.Find(document => document.CmsDocumentId == _documents.First().DocumentId);
            document?.Status.Should().Be(TrackerDocumentStatus.New);

            _tracker.Logs.Count.Should().Be(9);
        }

        [Fact]
        public async Task RegisterUnexpectedDocumentFailure_Registers()
        {
            await _tracker.Reset((It.IsAny<DateTime>(), _transactionId));
            await _tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            await _tracker.RegisterStatus((It.IsAny<DateTime>(), _pdfBlobNameArg.DocumentId, TrackerDocumentStatus.UnexpectedFailure, TrackerLogType.UnexpectedDocumentFailure));

            var document = _tracker.CmsDocuments.Find(document => document.CmsDocumentId == _documents.First().DocumentId);
            document?.Status.Should().Be(TrackerDocumentStatus.UnexpectedFailure);

            _tracker.Logs.Count.Should().Be(10);
        }

        [Fact]
        public async Task RegisterIndexed_RegistersIndexed()
        {
            await _tracker.Reset((It.IsAny<DateTime>(), _transactionId));
            await _tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            await _tracker.RegisterStatus(((It.IsAny<DateTime>(), _documents.First().DocumentId, TrackerDocumentStatus.Indexed, TrackerLogType.Indexed)));

            var document = _tracker.CmsDocuments.Find(document => document.CmsDocumentId == _documents.First().DocumentId);
            document?.Status.Should().Be(TrackerDocumentStatus.Indexed);

            _tracker.Logs.Count.Should().Be(10);
        }

        [Fact]
        public async Task RegisterIndexed_RegistersOcrAndIndexFailure()
        {
            await _tracker.Reset((It.IsAny<DateTime>(), _transactionId));
            await _tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            await _tracker.RegisterStatus((It.IsAny<DateTime>(), _documents.First().DocumentId, TrackerDocumentStatus.OcrAndIndexFailure, TrackerLogType.OcrAndIndexFailure));

            var document = _tracker.CmsDocuments.Find(document => document.CmsDocumentId == _documents.First().DocumentId);
            document?.Status.Should().Be(TrackerDocumentStatus.OcrAndIndexFailure);

            _tracker.Logs.Count.Should().Be(10);
        }

        [Fact]
        public async Task RegisterCompleted_RegistersCompleted()
        {
            await _tracker.Reset((It.IsAny<DateTime>(), _transactionId));
            await _tracker.RegisterCompleted((It.IsAny<DateTime>(), true));

            _tracker.Status.Should().Be(TrackerStatus.Completed);
            _tracker.ProcessingCompleted.Should().NotBeNull();

            _tracker.Logs.Count.Should().Be(2);
        }

        [Fact]
        public async Task RegisterFailed_RegistersFailed()
        {
            await _tracker.Reset((It.IsAny<DateTime>(), _transactionId));
            await _tracker.RegisterCompleted((It.IsAny<DateTime>(), false));

            _tracker.Status.Should().Be(TrackerStatus.Failed);

            _tracker.Logs.Count.Should().Be(2);
        }

        [Fact]
        public async Task GetDocuments_ReturnsDocuments()
        {
            _tracker.CmsDocuments = _trackerDocuments;
            var documents = await _tracker.GetDocuments();

            documents.Should().BeEquivalentTo(_trackerDocuments);
        }

        [Fact]
        public async Task AllDocumentsFailed_ReturnsTrueIfAllDocumentsFailed()
        {
            _tracker.CmsDocuments = new List<TrackerCmsDocumentDto> {
                new(_fixture.Create<Guid>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<PresentationFlagsDto>()) { Status = TrackerDocumentStatus.UnableToConvertToPdf},
                new(_fixture.Create<Guid>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(),  _fixture.Create<string>(), _fixture.Create<PresentationFlagsDto>()) { Status = TrackerDocumentStatus.UnexpectedFailure}
            };

            _tracker.PcdRequests = new List<TrackerPcdRequestDto>();

            var output = await _tracker.AllDocumentsFailed();

            output.Should().BeTrue();
        }

        [Fact]
        public async Task AllDocumentsFailed_ReturnsFalseIfAllDocumentsHaveNotFailed()
        {
            _tracker.CmsDocuments = new List<TrackerCmsDocumentDto> {
                new(_fixture.Create<Guid>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<PresentationFlagsDto>()) { Status = TrackerDocumentStatus.UnableToConvertToPdf},
                new(_fixture.Create<Guid>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<PresentationFlagsDto>()) { Status = TrackerDocumentStatus.UnexpectedFailure},
                new(_fixture.Create<Guid>(), _fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<DocumentTypeDto>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<PresentationFlagsDto>()) { Status = TrackerDocumentStatus.PdfUploadedToBlob},
            };

            _tracker.PcdRequests = new List<TrackerPcdRequestDto>();

            var output = await _tracker.AllDocumentsFailed();

            output.Should().BeFalse();
        }

        [Fact]
        public async Task Run_Tracker_Dispatches()
        {
            await TrackerEntity.Run(_mockDurableEntityContext.Object);

            _mockDurableEntityContext.Verify(context => context.DispatchAsync<TrackerEntity>());
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
            var entityStateResponse = new EntityStateResponse<TrackerEntity>() { EntityExists = false };
            _mockDurableEntityClient.Setup(
                client => client.ReadEntityStateAsync<TrackerEntity>(
                    It.Is<EntityId>(e => e.EntityName == nameof(TrackerEntity).ToLower() && e.EntityKey == _caseId.ToString()),
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
            var entityStateResponse = new EntityStateResponse<TrackerEntity>() { EntityExists = false };
            _mockDurableEntityClient.Setup(
                    client => client.ReadEntityStateAsync<TrackerEntity>(
                        It.Is<EntityId>(e => e.EntityName == nameof(TrackerEntity).ToLower() && e.EntityKey == _caseId.ToString()),
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
            TrackerEntity tracker = new TrackerEntity();
            await tracker.Reset((It.IsAny<DateTime>(), _transactionId));

            // Act
            var deltas = await tracker.SynchroniseDocuments(_synchroniseDocumentsArg);

            // Assert
            tracker.VersionId.Should().Be(1);
            tracker.CmsDocuments.Count.Should().Be(_documents.Count);
            deltas.CreatedCmsDocuments.Count.Should().Be(_documents.Count);
            deltas.UpdatedCmsDocuments.Count.Should().Be(0);
            deltas.DeletedCmsDocuments.Count.Should().Be(0);
            deltas.CreatedPcdRequests.Count.Should().Be(_documents.Count);
            deltas.UpdatedPcdRequests.Count.Should().Be(0);
            deltas.DeletedPcdRequests.Count.Should().Be(0);
            deltas.Any().Should().BeTrue();
            tracker.CmsDocuments[0].PolarisDocumentVersionId.Should().Be(1);
            tracker.CmsDocuments[1].PolarisDocumentVersionId.Should().Be(1);
            tracker.CmsDocuments[2].PolarisDocumentVersionId.Should().Be(1);
            tracker.PcdRequests[0].PolarisDocumentVersionId.Should().Be(1);
            tracker.PcdRequests[1].PolarisDocumentVersionId.Should().Be(1);
            tracker.PcdRequests[2].PolarisDocumentVersionId.Should().Be(1);
            tracker.Logs.Count.Should().Be(9);
        }

        [Fact]
        public async Task SynchroniseDocument_NoChangesWithExistingDocumentAndVersionIds()
        {
            // Arrange
            TrackerEntity tracker = new TrackerEntity();
            await tracker.Reset((It.IsAny<DateTime>(), _transactionId));
            await tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            tracker.CmsDocuments.ForEach(doc => doc.Status = TrackerDocumentStatus.Indexed);
            tracker.PcdRequests.ForEach(pcd => pcd.Status = TrackerDocumentStatus.Indexed);

            // Act 
            var deltas = await tracker.SynchroniseDocuments(_synchroniseDocumentsArg);

            // Assert
            tracker.VersionId.Should().Be(1);
            tracker.CmsDocuments.Count.Should().Be(_documents.Count);
            deltas.CreatedCmsDocuments.Count.Should().Be(0);
            deltas.UpdatedCmsDocuments.Count.Should().Be(0);
            deltas.DeletedCmsDocuments.Count.Should().Be(0);
            deltas.Any().Should().BeFalse();
            tracker.CmsDocuments[0].PolarisDocumentVersionId.Should().Be(1);
            tracker.CmsDocuments[1].PolarisDocumentVersionId.Should().Be(1);
            tracker.CmsDocuments[2].PolarisDocumentVersionId.Should().Be(1);
            tracker.Logs.Count.Should().Be(11);
        }

        [Fact]
        public async Task SynchroniseDocument_IndexedCmsDocumentAreRetained()
        {
            // Arrange
            TrackerEntity tracker = new TrackerEntity();
            await tracker.Reset((It.IsAny<DateTime>(), _transactionId));
            await tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            tracker.CmsDocuments.ForEach(doc => doc.Status = TrackerDocumentStatus.Indexed);
            tracker.PcdRequests.ForEach(pcd => pcd.Status = TrackerDocumentStatus.Indexed);

            // Act 
            var deltas = await tracker.SynchroniseDocuments(_synchroniseDocumentsArg);

            // Assert
            tracker.VersionId.Should().Be(1);
            tracker.CmsDocuments.Count.Should().Be(_documents.Count);
            deltas.CreatedCmsDocuments.Count.Should().Be(0);
            deltas.UpdatedCmsDocuments.Count.Should().Be(0);
            deltas.DeletedCmsDocuments.Count.Should().Be(0);
            deltas.Any().Should().BeFalse();
            tracker.CmsDocuments[0].PolarisDocumentVersionId.Should().Be(1);
            tracker.CmsDocuments[1].PolarisDocumentVersionId.Should().Be(1);
            tracker.CmsDocuments[2].PolarisDocumentVersionId.Should().Be(1);
            tracker.Logs.Count.Should().Be(11);
        }

        [Fact]
        public async Task SynchroniseDocument_FailedCmsDocumentAreReprocessed()
        {
            // Arrange
            TrackerEntity tracker = new TrackerEntity();
            await tracker.Reset((It.IsAny<DateTime>(), _transactionId));
            await tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            tracker.CmsDocuments.ForEach(doc => doc.Status = TrackerDocumentStatus.UnexpectedFailure);
            tracker.PcdRequests.ForEach(pcd => pcd.Status = TrackerDocumentStatus.Indexed);

            // Act 
            var deltas = await tracker.SynchroniseDocuments(_synchroniseDocumentsArg);

            // Assert
            tracker.VersionId.Should().Be(2);
            tracker.CmsDocuments.Count.Should().Be(_documents.Count);
            deltas.CreatedCmsDocuments.Count.Should().Be(0);
            deltas.UpdatedCmsDocuments.Count.Should().Be(3);
            deltas.DeletedCmsDocuments.Count.Should().Be(0);
            deltas.Any().Should().BeTrue();
            tracker.CmsDocuments[0].PolarisDocumentVersionId.Should().Be(2);
            tracker.CmsDocuments[1].PolarisDocumentVersionId.Should().Be(2);
            tracker.CmsDocuments[2].PolarisDocumentVersionId.Should().Be(2);
            tracker.Logs.Count.Should().Be(14);
        }

        [Fact]
        public async Task SynchroniseDocument_FailedPcdRequestsAreReprocessed()
        {
            // Arrange
            TrackerEntity tracker = new TrackerEntity();
            await tracker.Reset((It.IsAny<DateTime>(), _transactionId));
            await tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            tracker.CmsDocuments.ForEach(doc => doc.Status = TrackerDocumentStatus.Indexed);
            tracker.PcdRequests.ForEach(pcd => pcd.Status = TrackerDocumentStatus.UnexpectedFailure);

            // Act 
            var deltas = await tracker.SynchroniseDocuments(_synchroniseDocumentsArg);

            // Assert
            tracker.VersionId.Should().Be(2);
            tracker.CmsDocuments.Count.Should().Be(_documents.Count);
            deltas.CreatedCmsDocuments.Count.Should().Be(0);
            deltas.UpdatedCmsDocuments.Count.Should().Be(0);
            deltas.DeletedCmsDocuments.Count.Should().Be(0);
            deltas.CreatedPcdRequests.Count.Should().Be(0);
            deltas.UpdatedPcdRequests.Count.Should().Be(3);
            deltas.DeletedPcdRequests.Count.Should().Be(0);
            deltas.Any().Should().BeTrue();
            tracker.PcdRequests[0].PolarisDocumentVersionId.Should().Be(2);
            tracker.PcdRequests[1].PolarisDocumentVersionId.Should().Be(2);
            tracker.PcdRequests[2].PolarisDocumentVersionId.Should().Be(2);
            tracker.Logs.Count.Should().Be(14);
        }


        [Fact]
        public async Task SynchroniseDocument_ChangesWithUpdatedDocumentAndVersionIds()
        {
            // Arrange
            TrackerEntity tracker = new TrackerEntity();
            await tracker.Reset((It.IsAny<DateTime>(), _transactionId));
            await tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            tracker.CmsDocuments.ForEach(doc => doc.Status = TrackerDocumentStatus.Indexed);
            tracker.PcdRequests.ForEach(pcd => pcd.Status = TrackerDocumentStatus.Indexed);

            // Act 
            _synchroniseDocumentsArg.Documents[1].VersionId = 111111111;
            _synchroniseDocumentsArg.Documents[2].VersionId = 222222222;
            var deltas = await tracker.SynchroniseDocuments(_synchroniseDocumentsArg);

            // Assert
            tracker.VersionId.Should().Be(2);
            tracker.CmsDocuments.Count.Should().Be(_documents.Count);
            deltas.CreatedCmsDocuments.Count.Should().Be(0);
            deltas.UpdatedCmsDocuments.Count.Should().Be(2);
            deltas.DeletedCmsDocuments.Count.Should().Be(0);
            deltas.Any().Should().BeTrue();
            tracker.CmsDocuments[0].PolarisDocumentVersionId.Should().Be(1);
            tracker.CmsDocuments[1].PolarisDocumentVersionId.Should().Be(2);
            tracker.CmsDocuments[2].PolarisDocumentVersionId.Should().Be(2);

            tracker.Logs.Count.Should().Be(13);
        }

        [Fact]
        public async Task SynchroniseDocument_ChangesWithDeletedDocuments()
        {
            // Arrange
            TrackerEntity tracker = new TrackerEntity();
            await tracker.Reset((It.IsAny<DateTime>(), _transactionId));
            await tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            tracker.CmsDocuments.ForEach(doc => doc.Status = TrackerDocumentStatus.Indexed);
            tracker.PcdRequests.ForEach(doc => doc.Status = TrackerDocumentStatus.Indexed);

            // Act 
            _synchroniseDocumentsArg.Documents.RemoveAt(2);
            _synchroniseDocumentsArg.Documents.RemoveAt(1);
            _synchroniseDocumentsArg.PcdRequests.RemoveAt(2);
            var deltas = await tracker.SynchroniseDocuments(_synchroniseDocumentsArg);

            // Assert
            tracker.VersionId.Should().Be(2);
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
            tracker.PcdRequests[1].PolarisDocumentVersionId.Should().Be(1);
            tracker.Logs.Count.Should().Be(14);
        }

        [Fact]
        public async Task RegisterDocumentIds_TheNextDaysRun_DocumentsTheSame_ReturnsNothingToEvaluate()
        {
            await _tracker.Reset((It.IsAny<DateTime>(), _transactionId));
            await _tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            _tracker.CmsDocuments.Count.Should().Be(_documents.Count);

            var newDaysDocuments = new DocumentDto[3];
            _documents.CopyTo(newDaysDocuments);
            var newDaysDocumentIdsArg = _fixture.Build<SynchroniseDocumentsArg>()
                .With(a => a.CaseUrn, _caseUrn)
                .With(a => a.CaseId, _caseId)
                .With(a => a.Documents, newDaysDocuments.ToList())
                .With(a => a.CorrelationId, _correlationId)
                .Create();

            await _tracker.Reset((It.IsAny<DateTime>(), _transactionId));
            await _tracker.SynchroniseDocuments(newDaysDocumentIdsArg);

            using (new AssertionScope())
            {
                _tracker.CmsDocuments.Count.Should().Be(_documents.Count);
            }
        }

        [Fact]
        public async Task RegisterDocumentIds_TheNextDaysRun_DocumentsNotTheSame_ReturnsRecordsToEvaluate()
        {
            await _tracker.Reset((It.IsAny<DateTime>(), _transactionId));
            await _tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            _tracker.CmsDocuments.Count.Should().Be(_documents.Count);

            var newDaysDocuments = new List<DocumentDto> { _documents.First() };
            ////only one document in today's run, the next two should be removed from the tracker and in the evaluation results

            var newDaysDocumentIdsArg = _fixture.Build<SynchroniseDocumentsArg>()
                .With(a => a.CaseUrn, _caseUrn)
                .With(a => a.CaseId, _caseId)
                .With(a => a.Documents, newDaysDocuments)
                .With(a => a.CorrelationId, _correlationId)
                .Create();

            await _tracker.Reset((It.IsAny<DateTime>(), _transactionId));
            await _tracker.SynchroniseDocuments(newDaysDocumentIdsArg);

            using (new AssertionScope())
            {
                _tracker.CmsDocuments.Count.Should().Be(1);
            }
        }

        [Fact]
        public async Task RegisterDocumentIds_TheNextDaysRun_DocumentsTheSameExceptForANewVersionOfOneDoc_ReturnsOneRecordToEvaluate()
        {
            await _tracker.Reset((It.IsAny<DateTime>(), _transactionId));
            await _tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            _tracker.CmsDocuments.Count.Should().Be(_documents.Count);

            var newDaysDocuments = new DocumentDto[3];
            _documents.CopyTo(newDaysDocuments);
            var originalVersionId = newDaysDocuments[1].VersionId;
            var newVersionId = originalVersionId + 1;
            newDaysDocuments[1].VersionId = newVersionId;
            var modifiedDocumentId = newDaysDocuments[1].DocumentId;
            var newDaysDocumentIdsArg = _fixture.Build<SynchroniseDocumentsArg>()
                .With(a => a.CaseUrn, _caseUrn)
                .With(a => a.CaseId, _caseId)
                .With(a => a.Documents, newDaysDocuments.ToList())
                .With(a => a.CorrelationId, _correlationId)
                .Create();

            await _tracker.Reset((It.IsAny<DateTime>(), _transactionId));
            await _tracker.SynchroniseDocuments(newDaysDocumentIdsArg);

            using (new AssertionScope())
            {
                _tracker.CmsDocuments.Count.Should().Be(_documents.Count);
                var newVersion = _tracker.CmsDocuments.Find(x => x.CmsDocumentId == modifiedDocumentId);

                newVersion.Should().NotBeNull();
                newVersion?.CmsVersionId.Should().Be(newVersionId);
            }
        }

        [Fact]
        public async Task RegisterDocumentIds_TheNextDaysRun_OneDocumentRemovedAndOneANewVersion_ReturnsTwoRecordToEvaluate()
        {
            await _tracker.Reset((It.IsAny<DateTime>(), _transactionId));
            await _tracker.SynchroniseDocuments(_synchroniseDocumentsArg);
            _tracker.CmsDocuments.Count.Should().Be(_documents.Count);

            var newDaysDocuments = new List<DocumentDto>
            {
                _documents[1],
                _documents[2]
            };

            var documentRemovedFromCmsId = _documents[0].DocumentId;
            var originalVersionId = newDaysDocuments[0].VersionId;
            var newVersionId = originalVersionId + 1;
            newDaysDocuments[0].VersionId = newVersionId;
            var modifiedDocumentId = newDaysDocuments[0].DocumentId;

            var unmodifiedDocumentId = newDaysDocuments[1].DocumentId;
            var unmodifiedDocumentVersionId = newDaysDocuments[1].VersionId;

            var newDaysDocumentIdsArg = _fixture.Build<SynchroniseDocumentsArg>()
                .With(a => a.CaseUrn, _caseUrn)
                .With(a => a.CaseId, _caseId)
                .With(a => a.Documents, newDaysDocuments.ToList())
                .With(a => a.CorrelationId, _correlationId)
                .Create();

            await _tracker.Reset((It.IsAny<DateTime>(), _transactionId));
            await _tracker.SynchroniseDocuments(newDaysDocumentIdsArg);

            using (new AssertionScope())
            {
                _tracker.CmsDocuments.Count.Should().Be(2);
                var newVersion = _tracker.CmsDocuments.Find(x => x.CmsDocumentId == modifiedDocumentId);
                var unmodifiedDocument = _tracker.CmsDocuments.Find(x => x.CmsDocumentId == unmodifiedDocumentId);

                newVersion.Should().NotBeNull();
                newVersion?.CmsVersionId.Should().Be(newVersionId);

                unmodifiedDocument.Should().NotBeNull();
                unmodifiedDocument?.CmsVersionId.Should().Be(unmodifiedDocumentVersionId);

                var searchResultForDocumentRemovedFromCms = _tracker.CmsDocuments.Find(x => x.CmsDocumentId == documentRemovedFromCmsId);
                searchResultForDocumentRemovedFromCms.Should().BeNull();
            }
        }

        #endregion

        [Fact]
        public void TrackerEntityMapsToTrackerDto()
        {
            // Arrange
            _services.RegisterMapsterConfiguration();
            var trackerEntity = _fixture.Create<TrackerEntity>();

            // Act
            var trackerDto = trackerEntity.Adapt<TrackerDto>();

            // Assert
            trackerDto.Documents.Count.Should().Be(trackerEntity.CmsDocuments.Count + trackerEntity.PcdRequests.Count);
        }
    }
}
