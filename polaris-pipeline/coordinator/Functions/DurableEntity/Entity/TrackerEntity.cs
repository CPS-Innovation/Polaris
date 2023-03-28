using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Dto.Document;
using Common.Dto.Tracker;
using coordinator.Domain.Tracker;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace coordinator.Functions.DurableEntity.Entity
{

    [JsonObject(MemberSerialization.OptIn)]
    public class TrackerEntity : ITrackerEntity
    {
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("versionId")]
        public int VersionId { get; set; } = 1;

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public TrackerStatus Status { get; set; }

        [JsonProperty("processingCompleted")]
        public DateTime? ProcessingCompleted { get; set; }

        [JsonProperty("documents")]
        public List<TrackerDocumentDto> Documents { get; set; }

        [JsonProperty("logs")]
        public List<TrackerLogDto> Logs { get; set; }

        public Task Reset(string transactionid)
        {
            TransactionId = transactionid;
            ProcessingCompleted = null;
            Status = TrackerStatus.Running;
            Documents = Documents ?? new List<TrackerDocumentDto>();
            Logs = Logs ?? new List<TrackerLogDto>();

            Log(TrackerLogType.Initialised);

            return Task.CompletedTask;
        }

        public Task SetValue(TrackerEntity tracker)
        {
            Status = tracker.Status;
            ProcessingCompleted = tracker.ProcessingCompleted;
            Logs = new List<TrackerLogDto>();

            Documents = tracker.Documents;

            return Task.CompletedTask;
        }

        public Task<TrackerDocumentListDeltasDto> SynchroniseDocuments(SynchroniseDocumentsArg arg)
        {
            var deltas = new TrackerDocumentListDeltasDto
            {
                CreatedOrUpdated = new List<TrackerDocumentDto>(),
                Deleted = GetDocumentsToRemove(arg)
            };

            if (Documents == null)
                Documents = new List<TrackerDocumentDto>();

            RemoveDocuments(deltas.Deleted);
            CreateOrUpdateDocuments(arg.Documents, deltas);

            Status = TrackerStatus.DocumentsRetrieved;
            if (deltas.Any())
                VersionId++;

            if (deltas.Deleted.Any())
                Log(TrackerLogType.Deleted, null, $"{deltas.Deleted.Count} documents deleted");

            if (deltas.CreatedOrUpdated.Any())
                Log(TrackerLogType.DocumentsSynchronised, null, $"{deltas.CreatedOrUpdated.Count} documents created or updated");

            return Task.FromResult(deltas);
        }

        private void CreateOrUpdateDocuments(List<TransitionDocument> documents, TrackerDocumentListDeltasDto deltas)
        {
            var updatedDocuments = GetNewOrChangedDocuments(documents, deltas.Deleted);
            foreach (var updatedDocument in updatedDocuments)
            {
                TrackerDocumentDto trackerDocument = CreateTrackerDocument(updatedDocument);
                Documents.Add(trackerDocument);
                deltas.CreatedOrUpdated.Add(trackerDocument);
                Log(TrackerLogType.DocumentRetrieved, trackerDocument.CmsDocumentId);
            }
        }

        private List<DocumentVersionDto> GetDocumentsToRemove(SynchroniseDocumentsArg arg)
        {
            var removeDocuments = Documents.Where
                (
                    trackedDocument =>
                         !arg.Documents.Exists
                         (
                            x => x.DocumentId == trackedDocument.CmsDocumentId &&
                                 x.VersionId == trackedDocument.CmsVersionId
                         )
                );
            List<DocumentVersionDto> documentsToRemove 
                = removeDocuments
                    .Select(d => new DocumentVersionDto(d.CmsDocumentId, d.CmsVersionId))
                    .ToList();

            return documentsToRemove;
        }

        private IEnumerable<TransitionDocument> GetNewOrChangedDocuments(List<TransitionDocument> transitionDocuments, List<DocumentVersionDto> documentsToRemove)
        {
            var newDocuments = from cmsDocument in transitionDocuments
                               where !documentsToRemove.Exists
                               (
                                   x => x.DocumentId == cmsDocument.DocumentId &&
                                        x.VersionId == cmsDocument.VersionId
                               )
                               let item = Documents.Find(x => x.CmsDocumentId == cmsDocument.DocumentId)
                               where item == null
                               select cmsDocument;
            return newDocuments;
        }

        private void RemoveDocuments(List<DocumentVersionDto> documentsToRemove)
        {
            foreach (var item in
                     documentsToRemove.Select(invalidDocument =>
                         Documents.Find(x => x.CmsDocumentId == invalidDocument.DocumentId && x.CmsVersionId == invalidDocument.VersionId)))
            {
                Log(TrackerLogType.Deleted, item.CmsDocumentId);
                Documents.Remove(item);
            }
        }

        private TrackerDocumentDto CreateTrackerDocument(TransitionDocument document)
        {
            return new TrackerDocumentDto(
                document.PolarisDocumentId,
                document.DocumentId,
                document.VersionId,
                document.CmsDocType,
                document.MimeType,
                document.FileExtension,
                document.CreatedDate,
                document.OriginalFileName,
                document.PresentationFlags);
        }

        public Task RegisterPdfBlobName(RegisterPdfBlobNameArg arg)
        {
            var document = Documents.Find(document => document.CmsDocumentId.Equals(arg.DocumentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
            {
                document.PdfBlobName = arg.BlobName;
                document.Status = TrackerDocumentStatus.PdfUploadedToBlob;
                document.IsPdfAvailable = true;
            }

            Log(TrackerLogType.RegisteredPdfBlobName, arg.DocumentId);

            return Task.CompletedTask;
        }

        public Task RegisterBlobAlreadyProcessed(RegisterPdfBlobNameArg arg)
        {
            var document = Documents.Find(document => document.CmsDocumentId.Equals(arg.DocumentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
            {
                document.PdfBlobName = arg.BlobName;
                document.Status = TrackerDocumentStatus.DocumentAlreadyProcessed;
            }

            Log(TrackerLogType.DocumentAlreadyProcessed, arg.DocumentId);

            return Task.CompletedTask;
        }

        public Task RegisterUnableToConvertDocumentToPdf(string documentId)
        {
            var document = Documents.Find(document => document.CmsDocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
                document.Status = TrackerDocumentStatus.UnableToConvertToPdf;

            Log(TrackerLogType.UnableToConvertDocumentToPdf, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterUnexpectedPdfDocumentFailure(string documentId)
        {
            var document = Documents.Find(document => document.CmsDocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
                document.Status = TrackerDocumentStatus.UnexpectedFailure;

            Log(TrackerLogType.UnexpectedDocumentFailure, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterIndexed(string documentId)
        {
            var document = Documents.Find(document => document.CmsDocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
                document.Status = TrackerDocumentStatus.Indexed;

            Log(TrackerLogType.Indexed, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterOcrAndIndexFailure(string documentId)
        {
            var document = Documents.Find(document => document.CmsDocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
                document.Status = TrackerDocumentStatus.OcrAndIndexFailure;

            Log(TrackerLogType.OcrAndIndexFailure, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterCompleted()
        {
            Status = TrackerStatus.Completed;
            Log(TrackerLogType.Completed);
            ProcessingCompleted = DateTime.Now;

            return Task.CompletedTask;
        }

        public Task RegisterFailed()
        {
            Status = TrackerStatus.Failed;
            Log(TrackerLogType.Failed);
            ProcessingCompleted = DateTime.Now;

            return Task.CompletedTask;
        }

        public Task RegisterDeleted()
        {
            ClearState(TrackerStatus.Deleted);
            Log(TrackerLogType.Deleted);
            ProcessingCompleted = DateTime.Now;

            return Task.CompletedTask;
        }

        public Task<List<TrackerDocumentDto>> GetDocuments()
        {
            return Task.FromResult(Documents);
        }

        public Task ClearDocuments()
        {
            Documents.Clear();

            return Task.CompletedTask;
        }

        public Task<bool> AllDocumentsFailed()
        {
            return Task.FromResult(
                Documents.All(d => d.Status is TrackerDocumentStatus.UnableToConvertToPdf or TrackerDocumentStatus.UnexpectedFailure));
        }

        private void ClearState(TrackerStatus status)
        {
            Status = status;
            Documents = new List<TrackerDocumentDto>();
            Logs = new List<TrackerLogDto>();
            ProcessingCompleted = null; //reset the processing date
        }

        private void Log(TrackerLogType status, string cmsDocumentId = null, string description = null)
        {
            TrackerLogDto item = new TrackerLogDto
            {
                LogType = status.ToString(),
                TimeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffzzz"),
                Description = description,
                CmsDocumentId = cmsDocumentId
            };
            Logs.Insert(0, item);
        }

        [FunctionName(nameof(TrackerEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext context)
        {
            return context.DispatchAsync<TrackerEntity>();
        }
    }
}