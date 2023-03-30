using Common.Dto.Tracker;
using coordinator.Domain.Tracker;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coordinator.Functions.DurableEntity.Entity
{

    [JsonObject(MemberSerialization.OptIn)]
    public class TrackerEntity : ITrackerEntity
    {
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [JsonProperty("versionId")]
        public int VersionId { get; set; } = 0;

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public TrackerStatus Status { get; set; }

        [JsonProperty("documentsRetrieved")]
        public DateTime? DocumentsRetrieved { get; set; }

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
            var incomingDocuments = arg.Documents;

            if (Documents == null)
                Documents = new List<TrackerDocumentDto>();

            var (createdDocuments, updatedDocuments, deletedDocuments) = GetDeltaDocuments(incomingDocuments);

            TrackerDocumentListDeltasDto deltas = new TrackerDocumentListDeltasDto
            {
                Created = CreateDocuments(createdDocuments),
                Updated = UpdateDocuments(updatedDocuments),
                Deleted = DeleteDocuments(deletedDocuments)
            };

            Status = TrackerStatus.DocumentsRetrieved;
            DocumentsRetrieved = DateTime.Now;
            VersionId = deltas.Any() ? VersionId+1 : Math.Max(VersionId, 1);

            Log(TrackerLogType.DocumentsSynchronised, null, $"{deltas.Created.Count} created, {deltas.Updated.Count} updated, {deltas.Deleted.Count} deleted");

            return Task.FromResult(deltas);
        }

        private (List<TransitionDocument>, List<TransitionDocument>, List<string>) GetDeltaDocuments(List<TransitionDocument> transitionDocuments)
        {
            var existingCmsDocumentIds = Documents.Select(d => d.CmsDocumentId).ToList();
            var newDocuments =
                transitionDocuments
                    .Where(d => !existingCmsDocumentIds.Contains(d.CmsDocumentId))
                    .ToList();

            var existingCmsDocumentIdVersions = Documents.Select(d => (d.CmsDocumentId, d.CmsVersionId)).ToList();
            var updatedDocuments =
                transitionDocuments
                    .Where(d => existingCmsDocumentIds.Contains(d.CmsDocumentId))
                    .Where(d => !existingCmsDocumentIdVersions.Contains((d.CmsDocumentId, d.CmsVersionId)))
                    .ToList();

            var deletedCmsDocumentIdsToRemove
                = Documents.Where(trackedDocument => !transitionDocuments.Exists(x => x.CmsDocumentId == trackedDocument.CmsDocumentId))
                    .Select(d => d.CmsDocumentId)
                    .ToList();

            return (newDocuments, updatedDocuments, deletedCmsDocumentIdsToRemove);
        }

        private List<TrackerDocumentDto> CreateDocuments(List<TransitionDocument> createdDocuments)
        {
            List<TrackerDocumentDto> newDocuments = new List<TrackerDocumentDto>();

            foreach (var newDocument in createdDocuments)
            {
                TrackerDocumentDto trackerDocument 
                    = new TrackerDocumentDto
                    (
                        newDocument.PolarisDocumentId,
                        1,
                        newDocument.CmsDocumentId,
                        newDocument.CmsVersionId,
                        newDocument.CmsDocType,
                        newDocument.MimeType,
                        newDocument.FileExtension,
                        newDocument.CreatedDate,
                        newDocument.OriginalFileName,
                        newDocument.PresentationFlags
                    );

                Documents.Add(trackerDocument);
                newDocuments.Add(trackerDocument);
                Log(TrackerLogType.DocumentRetrieved, trackerDocument.CmsDocumentId);
            }

            return newDocuments;
        }

        private List<TrackerDocumentDto> UpdateDocuments(List<TransitionDocument> updatedDocuments)
        {
            List<TrackerDocumentDto> changedDocuments = new List<TrackerDocumentDto>();

            foreach (var updatedDocument in updatedDocuments)
            {
                TrackerDocumentDto trackerDocument = Documents.Find(d => d.CmsDocumentId == updatedDocument.CmsDocumentId);

                trackerDocument.PolarisDocumentVersionId++;
                trackerDocument.CmsVersionId = updatedDocument.CmsVersionId;
                trackerDocument.CmsDocType = updatedDocument.CmsDocType;
                trackerDocument.CmsMimeType = updatedDocument.MimeType;
                trackerDocument.CmsFileExtension = updatedDocument.FileExtension;
                trackerDocument.CmsFileCreatedDate = updatedDocument.CreatedDate;
                trackerDocument.CmsOriginalFileName = updatedDocument.OriginalFileName;
                trackerDocument.PresentationFlags = updatedDocument.PresentationFlags;

                changedDocuments.Add(trackerDocument);

                Log(TrackerLogType.DocumentRetrieved, trackerDocument.CmsDocumentId);
            }

            return changedDocuments;
        }

        private List<TrackerDocumentDto> DeleteDocuments(List<string> documentIdsToDelete)
        {
            var deleteDocuments 
                = Documents
                    .Where(d => documentIdsToDelete.Contains(d.CmsDocumentId))
                    .ToList();

            foreach (var document in deleteDocuments)
            {
                Log(TrackerLogType.Deleted, document.CmsDocumentId);
                Documents.Remove(document);
            }

            return deleteDocuments;
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
            ProcessingCompleted = DateTime.Now;
            Log(TrackerLogType.Completed);

            return Task.CompletedTask;
        }

        public Task RegisterFailed()
        {
            Status = TrackerStatus.Failed;
            ProcessingCompleted = DateTime.Now;
            Log(TrackerLogType.Failed);

            return Task.CompletedTask;
        }

        public Task RegisterDeleted()
        {
            ClearState(TrackerStatus.Deleted);
            ProcessingCompleted = DateTime.Now;
            Log(TrackerLogType.Deleted);

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
            DocumentsRetrieved = null;
            ProcessingCompleted = null;
            Logs = new List<TrackerLogDto>();
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