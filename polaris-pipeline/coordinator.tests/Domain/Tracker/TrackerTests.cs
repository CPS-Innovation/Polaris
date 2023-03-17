using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Common.Domain.DocumentEvaluation;
using Common.Domain.DocumentExtraction;
using Common.Domain.Pipeline;
using coordinator.Domain.Tracker;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace coordinator.tests.Domain.Tracker
{
    public class TrackerTests
    {
        private readonly Fixture _fixture;
        private readonly string _transactionId;
        private readonly List<IncomingDocument> _incomingDocuments;
        private readonly RegisterPdfBlobNameArg _pdfBlobNameArg;
        private readonly RegisterDocumentIdsArg _registerDocumentIdsArg;
        private readonly List<TrackerDocument> _trackerDocuments;
        private readonly string _caseUrn;
        private readonly long _caseId;
        private readonly Guid _correlationId;
        private readonly EntityStateResponse<coordinator.Domain.Tracker.Tracker> _entityStateResponse;

        private readonly Mock<IDurableEntityContext> _mockDurableEntityContext;
        private readonly Mock<IDurableEntityClient> _mockDurableEntityClient;
        private readonly Mock<ILogger> _mockLogger;

        private readonly coordinator.Domain.Tracker.Tracker _tracker;
        private readonly coordinator.Functions.ClientFunctions.TrackerStatus _trackerStatus;

        public TrackerTests()
        {
            _fixture = new Fixture();
            _transactionId = _fixture.Create<string>();
            _incomingDocuments = _fixture.CreateMany<IncomingDocument>(3).ToList();
            _correlationId = _fixture.Create<Guid>();
            _pdfBlobNameArg = _fixture.Build<RegisterPdfBlobNameArg>()
                                .With(a => a.DocumentId, _incomingDocuments.First().DocumentId)
                                .With(a => a.VersionId, _incomingDocuments.First().VersionId)
                                .Create();
            _trackerDocuments = _fixture.Create<List<TrackerDocument>>();
            _caseUrn = _fixture.Create<string>();
            _caseId = _fixture.Create<long>();
            _registerDocumentIdsArg = _fixture.Build<RegisterDocumentIdsArg>()
                .With(a => a.CaseUrn, _caseUrn)
                .With(a => a.CaseId, _caseId)
                .With(a => a.IncomingDocuments, _incomingDocuments)
                .With(a => a.CorrelationId, _correlationId)
                .Create();
            _entityStateResponse = new EntityStateResponse<coordinator.Domain.Tracker.Tracker>() { EntityExists = true };

            _mockDurableEntityContext = new Mock<IDurableEntityContext>();
            _mockDurableEntityClient = new Mock<IDurableEntityClient>();
            _mockLogger = new Mock<ILogger>();

            _mockDurableEntityClient.Setup(
                client => client.ReadEntityStateAsync<coordinator.Domain.Tracker.Tracker>(
                    It.Is<EntityId>(e => e.EntityName == nameof(coordinator.Domain.Tracker.Tracker).ToLower() && e.EntityKey == _caseId.ToString()),
                    null, null))
                .ReturnsAsync(_entityStateResponse);

            _tracker = new coordinator.Domain.Tracker.Tracker();
            _trackerStatus = new coordinator.Functions.ClientFunctions.TrackerStatus();
        }

        [Fact]
        public async Task Initialise_Initialises()
        {
            await _tracker.Initialise(_transactionId);

            _tracker.TransactionId.Should().Be(_transactionId);
            _tracker.Documents.Should().NotBeNull();
            _tracker.Logs.Should().NotBeNull();
            _tracker.Status.Should().Be(TrackerStatus.Running);

            _tracker.Logs.Count.Should().Be(1);
        }

        [Fact]
        public async Task RegisterPdfBlobName_RegistersPdfBlobName()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_registerDocumentIdsArg);
            await _tracker.RegisterPdfBlobName(_pdfBlobNameArg);

            var document = _tracker.Documents.Find(document => document.CmsDocumentId == _incomingDocuments.First().DocumentId);
            document?.PdfBlobName.Should().Be(_pdfBlobNameArg.BlobName);
            document?.Status.Should().Be(DocumentStatus.PdfUploadedToBlob);

            _tracker.Logs.Count.Should().Be(3);
        }

        [Fact]
        public async Task RegisterDocumentNotFoundInDDEI_Registers()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_registerDocumentIdsArg);
            await _tracker.RegisterDocumentNotFoundInDDEI(_pdfBlobNameArg.DocumentId);

            var document = _tracker.Documents.Find(document => document.CmsDocumentId == _incomingDocuments.First().DocumentId);
            document?.Status.Should().Be(DocumentStatus.NotFoundInDDEI);

            _tracker.Logs.Count.Should().Be(3);
        }
        
        [Fact]
        public async Task RegisterIfRequired_EvaluatedDocuments_RequireProcessing()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_registerDocumentIdsArg);
            await _tracker.ProcessEvaluatedDocuments();

            _tracker.Logs.Count.Should().Be(3);
        }
        
        [Fact]
        public async Task RegisterDocumentAsAlreadyProcessed_Registers()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_registerDocumentIdsArg);
            await _tracker.RegisterBlobAlreadyProcessed(new RegisterPdfBlobNameArg(_pdfBlobNameArg.DocumentId, _pdfBlobNameArg.VersionId, _pdfBlobNameArg.BlobName));

            var document = _tracker.Documents.Find(document => document.CmsDocumentId == _pdfBlobNameArg.DocumentId);
            document?.Status.Should().Be(DocumentStatus.DocumentAlreadyProcessed);

            _tracker.Logs.Count.Should().Be(3);
        }
        
        [Fact]
        public async Task RegisterDocumentAsFailedPDFConversion_Registers()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_registerDocumentIdsArg);
            await _tracker.RegisterUnableToConvertDocumentToPdf(_pdfBlobNameArg.DocumentId);

            var document = _tracker.Documents.Find(document => document.CmsDocumentId == _pdfBlobNameArg.DocumentId);
            document?.Status.Should().Be(DocumentStatus.UnableToConvertToPdf);

            _tracker.Logs.Count.Should().Be(3);
        }

        [Fact]
        public async Task Initialisation_SetsDocumentStatusToNone()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_registerDocumentIdsArg);

            var document = _tracker.Documents.Find(document => document.CmsDocumentId == _incomingDocuments.First().DocumentId);
            document?.Status.Should().Be(DocumentStatus.None);

            _tracker.Logs.Count.Should().Be(2);
        }

        [Fact]
        public async Task RegisterUnexpectedDocumentFailure_Registers()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_registerDocumentIdsArg);
            await _tracker.RegisterUnexpectedPdfDocumentFailure(_pdfBlobNameArg.DocumentId);

            var document = _tracker.Documents.Find(document => document.CmsDocumentId == _incomingDocuments.First().DocumentId);
            document?.Status.Should().Be(DocumentStatus.UnexpectedFailure);

            _tracker.Logs.Count.Should().Be(3);
        }

        [Fact]
        public async Task RegisterNoDocumentsFoundInDDEI_RegistersNoDocumentsFoundInDDEI()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterNoDocumentsFoundInDDEI();

            _tracker.Status.Should().Be(TrackerStatus.NoDocumentsFoundInDDEI);

            _tracker.Logs.Count.Should().Be(2);
        }

        [Fact]
        public async Task RegisterIndexed_RegistersIndexed()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_registerDocumentIdsArg);
            await _tracker.RegisterIndexed(_incomingDocuments.First().DocumentId);

            var document = _tracker.Documents.Find(document => document.CmsDocumentId == _incomingDocuments.First().DocumentId);
            document?.Status.Should().Be(DocumentStatus.Indexed);

            _tracker.Logs.Count.Should().Be(3);
        }

        [Fact]
        public async Task RegisterIndexed_RegistersOcrAndIndexFailure()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_registerDocumentIdsArg);
            await _tracker.RegisterOcrAndIndexFailure(_incomingDocuments.First().DocumentId);

            var document = _tracker.Documents.Find(document => document.CmsDocumentId == _incomingDocuments.First().DocumentId);
            document?.Status.Should().Be(DocumentStatus.OcrAndIndexFailure);

            _tracker.Logs.Count.Should().Be(3);
        }

        [Fact]
        public async Task RegisterCompleted_RegistersCompleted()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterCompleted();

            _tracker.Status.Should().Be(TrackerStatus.Completed);

            _tracker.Logs.Count.Should().Be(2);
        }

        [Fact]
        public async Task RegisterFailed_RegistersFailed()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterFailed();

            _tracker.Status.Should().Be(TrackerStatus.Failed);

            _tracker.Logs.Count.Should().Be(2);
        }
        
        [Fact]
        public async Task GetDocuments_ReturnsDocuments()
        {
            _tracker.Documents = _trackerDocuments;
            var documents = await _tracker.GetDocuments();

            documents.Should().BeEquivalentTo(_trackerDocuments);
        }

        [Fact]
        public async Task AllDocumentsFailed_ReturnsTrueIfAllDocumentsFailed()
        {
            _tracker.Documents = new List<TrackerDocument> {
                new(_fixture.Create<Guid>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<CmsDocType>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>()) { Status = DocumentStatus.NotFoundInDDEI},
                new(_fixture.Create<Guid>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<CmsDocType>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>()) { Status = DocumentStatus.UnableToConvertToPdf},
                new(_fixture.Create<Guid>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<CmsDocType>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>()) { Status = DocumentStatus.UnexpectedFailure}
            };

            var output = await _tracker.AllDocumentsFailed();

            output.Should().BeTrue();
        }

        [Fact]
        public async Task AllDocumentsFailed_ReturnsFalseIfAllDocumentsHaveNotFailed()
        {
            _tracker.Documents = new List<TrackerDocument> {
                new(_fixture.Create<Guid>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<CmsDocType>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>()) { Status = DocumentStatus.NotFoundInDDEI},
                new(_fixture.Create<Guid>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<CmsDocType>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>()) { Status = DocumentStatus.UnableToConvertToPdf},
                new(_fixture.Create<Guid>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<CmsDocType>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>()) { Status = DocumentStatus.UnexpectedFailure},
                new(_fixture.Create<Guid>(), _fixture.Create<string>(), _fixture.Create<long>(), _fixture.Create<CmsDocType>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>()) { Status = DocumentStatus.PdfUploadedToBlob},
            };

            var output = await _tracker.AllDocumentsFailed();

            output.Should().BeFalse();
        }

        [Fact]
        public async Task IsAlreadyProcessed_ReturnsTrueIfStatusIsCompleted()
        {
            _tracker.Status = TrackerStatus.Completed;

            var isAlreadyProcessed = await _tracker.IsAlreadyProcessed();

            isAlreadyProcessed.Should().BeTrue();
        }

        [Fact]
        public async Task IsAlreadyProcessed_ReturnsTrueIfStatusIsNoDocumentsFoundInDDEI()
        {
            _tracker.Status = TrackerStatus.NoDocumentsFoundInDDEI;

            var isAlreadyProcessed = await _tracker.IsAlreadyProcessed();

            isAlreadyProcessed.Should().BeTrue();
        }

        [Fact]
        public async Task IsAlreadyProcessed_ReturnsFalseIfStatusIsNotCompletedAndNotNoDocumentsFoundInDDEI()
        {
            _tracker.Status = TrackerStatus.NotStarted;

            var isAlreadyProcessed = await _tracker.IsAlreadyProcessed();

            isAlreadyProcessed.Should().BeFalse();
        }

        [Fact]
        public async Task Run_Tracker_Dispatches()
        {
            await coordinator.Domain.Tracker.Tracker.Run(_mockDurableEntityContext.Object);

            _mockDurableEntityContext.Verify(context => context.DispatchAsync<coordinator.Domain.Tracker.Tracker>());
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
            var response  = await _trackerStatus.HttpStart(message, _caseUrn, _caseId.ToString(), _mockDurableEntityClient.Object, _mockLogger.Object);

            var okObjectResult = response as OkObjectResult;

            okObjectResult?.Value.Should().Be(_entityStateResponse.EntityState);
        }

        [Fact]
        public async Task HttpStart_TrackerStatus_ReturnsNotFoundIfEntityNotFound()
        {
            var entityStateResponse = new EntityStateResponse<coordinator.Domain.Tracker.Tracker>() { EntityExists = false };
            _mockDurableEntityClient.Setup(
                client => client.ReadEntityStateAsync<coordinator.Domain.Tracker.Tracker>(
                    It.Is<EntityId>(e => e.EntityName == nameof(coordinator.Domain.Tracker.Tracker).ToLower() && e.EntityKey == _caseId.ToString()),
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
            var entityStateResponse = new EntityStateResponse<coordinator.Domain.Tracker.Tracker>() { EntityExists = false };
            _mockDurableEntityClient.Setup(
                    client => client.ReadEntityStateAsync<coordinator.Domain.Tracker.Tracker>(
                        It.Is<EntityId>(e => e.EntityName == nameof(coordinator.Domain.Tracker.Tracker).ToLower() && e.EntityKey == _caseId.ToString()),
                        null, null))
                .ReturnsAsync(entityStateResponse);

            var message = new HttpRequestMessage();
            var response = await _trackerStatus.HttpStart(message, _caseUrn, _caseId.ToString(), _mockDurableEntityClient.Object, _mockLogger.Object);

            response.Should().BeOfType<BadRequestObjectResult>();
        }
        
        #region IsStale Tests
        
        [Fact]
        public async Task IsStaleCheck_ReturnsTrue_WhenForceRefreshIsPassedAsTrue()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_registerDocumentIdsArg);
            await _tracker.RegisterCompleted();

            var result = await _tracker.IsStale(true);

            result.Should().BeTrue();
        }
        
        [Fact]
        public async Task IsStaleCheck_ReturnsFalse_WhenForceRefreshIsPassedAsFalse()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_registerDocumentIdsArg);

            var result = await _tracker.IsStale(false);

            result.Should().BeFalse();
        }
        
        [Fact]
        public async Task IsStaleCheck_ReturnsTrue_WhenForceRefreshIsPassedAsFalse_ButTheTrackerStatusIsFailed()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_registerDocumentIdsArg);
            await _tracker.RegisterFailed();

            var result = await _tracker.IsStale(false);

            result.Should().BeTrue();
            _tracker.Logs.Count.Should().Be(3);
        }
        
        [Fact]
        public async Task IsStaleCheck_ReturnsFalse_WhenForceRefreshIsPassedAsFalse_ButTheTrackerStatusIsRunning()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_registerDocumentIdsArg);
            _tracker.Status = TrackerStatus.Running;

            var result = await _tracker.IsStale(false);

            result.Should().BeFalse();
        }
        
        [Fact]
        public async Task IsStaleCheck_ReturnsFalse_WhenForceRefreshIsPassedAsTrue_ButTheTrackerStatusIsRunning()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_registerDocumentIdsArg);
            _tracker.Status = TrackerStatus.Running;

            var result = await _tracker.IsStale(true);

            result.Should().BeFalse();
        }
        
        [Fact]
        public async Task IsStaleCheck_ReturnsFalse_WhenForceRefreshIsPassedAsFalse_AndTheProcessingDateHasNotBeenSet()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_registerDocumentIdsArg);
            _tracker.ProcessingCompleted = null;

            var result = await _tracker.IsStale(false);

            result.Should().BeFalse();
        }
        
        [Fact]
        public async Task IsStaleCheck_ReturnsFalse_WhenForceRefreshIsPassedAsFalse_AndTheProcessingDateIsTheSameAsToday()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_registerDocumentIdsArg);
            await _tracker.RegisterCompleted();

            var result = await _tracker.IsStale(false);

            result.Should().BeFalse();
        }
        
        [Fact]
        public async Task IsStaleCheck_ReturnsTrue_WhenForceRefreshIsPassedAsFalse_AndTheProcessingDateIsNotTheSameAsToday()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_registerDocumentIdsArg);
            await _tracker.RegisterCompleted();

            _tracker.ProcessingCompleted = _tracker.ProcessingCompleted.GetValueOrDefault(DateTime.Now).AddDays(-1);

            var result = await _tracker.IsStale(false);

            result.Should().BeTrue();
        }
        
        #endregion
        
        #region RegisterDocumentIds
        
        [Fact]
        public async Task RegisterDocumentIds_ForTheFirstTime_HoldsTheCorrectNumberOfDocs()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_registerDocumentIdsArg);

            _tracker.Documents.Count.Should().Be(_incomingDocuments.Count);

            _tracker.Logs.Count.Should().Be(2);
        }
        
        [Fact]
        public async Task RegisterDocumentIds_TheNextDaysRun_IncomingDocumentsTheSame_ReturnsNothingToEvaluate()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_registerDocumentIdsArg);
            _tracker.Documents.Count.Should().Be(_incomingDocuments.Count);
            
            var newDaysIncomingDocuments = new IncomingDocument[3];
            _incomingDocuments.CopyTo(newDaysIncomingDocuments);
            var newDaysDocumentIdsArg = _fixture.Build<RegisterDocumentIdsArg>()
                .With(a => a.CaseUrn, _caseUrn)
                .With(a => a.CaseId, _caseId)
                .With(a => a.IncomingDocuments, newDaysIncomingDocuments.ToList())
                .With(a => a.CorrelationId, _correlationId)
                .Create();

            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(newDaysDocumentIdsArg);

            using (new AssertionScope())
            {
                _tracker.Documents.Count.Should().Be(_incomingDocuments.Count);
            }
        }
        
        [Fact]
        public async Task RegisterDocumentIds_TheNextDaysRun_IncomingDocumentsNotTheSame_ReturnsRecordsToEvaluate()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_registerDocumentIdsArg);
            _tracker.Documents.Count.Should().Be(_incomingDocuments.Count);
            
            var newDaysIncomingDocuments = new List<IncomingDocument> {_incomingDocuments.First()};
            ////only one document in today's run, the next two should be removed from the tracker and in the evaluation results
            
            var newDaysDocumentIdsArg = _fixture.Build<RegisterDocumentIdsArg>()
                .With(a => a.CaseUrn, _caseUrn)
                .With(a => a.CaseId, _caseId)
                .With(a => a.IncomingDocuments, newDaysIncomingDocuments)
                .With(a => a.CorrelationId, _correlationId)
                .Create();

            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(newDaysDocumentIdsArg);

            using (new AssertionScope())
            {
                _tracker.Documents.Count.Should().Be(1);
            }
        }
        
        [Fact]
        public async Task RegisterDocumentIds_TheNextDaysRun_IncomingDocumentsTheSameExceptForANewVersionOfOneDoc_ReturnsOneRecordToEvaluate()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_registerDocumentIdsArg);
            _tracker.Documents.Count.Should().Be(_incomingDocuments.Count);
            
            var newDaysIncomingDocuments = new IncomingDocument[3];
            _incomingDocuments.CopyTo(newDaysIncomingDocuments);
            var originalVersionId = newDaysIncomingDocuments[1].VersionId;
            var newVersionId = originalVersionId + 1;
            newDaysIncomingDocuments[1].VersionId = newVersionId;
            var modifiedDocumentId = newDaysIncomingDocuments[1].DocumentId;
            var newDaysDocumentIdsArg = _fixture.Build<RegisterDocumentIdsArg>()
                .With(a => a.CaseUrn, _caseUrn)
                .With(a => a.CaseId, _caseId)
                .With(a => a.IncomingDocuments, newDaysIncomingDocuments.ToList())
                .With(a => a.CorrelationId, _correlationId)
                .Create();

            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(newDaysDocumentIdsArg);

            using (new AssertionScope())
            {
                _tracker.Documents.Count.Should().Be(_incomingDocuments.Count);
                var newVersion = _tracker.Documents.Find(x => x.CmsDocumentId == modifiedDocumentId);

                newVersion.Should().NotBeNull();
                newVersion?.CmsVersionId.Should().Be(newVersionId);
            }
        }
        
        [Fact]
        public async Task RegisterDocumentIds_TheNextDaysRun_OneDocumentRemovedAndOneANewVersion_ReturnsTwoRecordToEvaluate()
        {
            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(_registerDocumentIdsArg);
            _tracker.Documents.Count.Should().Be(_incomingDocuments.Count);
            
            var newDaysIncomingDocuments = new List<IncomingDocument>
            {
                _incomingDocuments[1],
                _incomingDocuments[2]
            };

            var documentRemovedFromCmsId = _incomingDocuments[0].DocumentId;
            var originalVersionId = newDaysIncomingDocuments[0].VersionId;
            var newVersionId = originalVersionId + 1;
            newDaysIncomingDocuments[0].VersionId = newVersionId;
            var modifiedDocumentId = newDaysIncomingDocuments[0].DocumentId;

            var unmodifiedDocumentId = newDaysIncomingDocuments[1].DocumentId;
            var unmodifiedDocumentVersionId = newDaysIncomingDocuments[1].VersionId;
            
            var newDaysDocumentIdsArg = _fixture.Build<RegisterDocumentIdsArg>()
                .With(a => a.CaseUrn, _caseUrn)
                .With(a => a.CaseId, _caseId)
                .With(a => a.IncomingDocuments, newDaysIncomingDocuments.ToList())
                .With(a => a.CorrelationId, _correlationId)
                .Create();

            await _tracker.Initialise(_transactionId);
            await _tracker.RegisterDocumentIds(newDaysDocumentIdsArg);

            using (new AssertionScope())
            {
                _tracker.Documents.Count.Should().Be(2);
                var newVersion = _tracker.Documents.Find(x => x.CmsDocumentId == modifiedDocumentId);
                var unmodifiedDocument = _tracker.Documents.Find(x => x.CmsDocumentId == unmodifiedDocumentId);

                newVersion.Should().NotBeNull();
                newVersion?.CmsVersionId.Should().Be(newVersionId);

                unmodifiedDocument.Should().NotBeNull();
                unmodifiedDocument?.CmsVersionId.Should().Be(unmodifiedDocumentVersionId);

                var searchResultForDocumentRemovedFromCms = _tracker.Documents.Find(x => x.CmsDocumentId == documentRemovedFromCmsId);
                searchResultForDocumentRemovedFromCms.Should().BeNull();
            }
        }
        
        #endregion
    }
}
