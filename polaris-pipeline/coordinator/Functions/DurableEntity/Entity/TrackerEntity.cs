using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using Common.Dto.Tracker;
using coordinator.Domain.Tracker;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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

        [JsonProperty("pcdRequests")]
        public List<TrackerPcdRequestDto> PcdRequests { get; set; }

        [JsonProperty("logs")]
        public List<TrackerLogDto> Logs { get; set; }

        public Task Reset(string transactionid)
        {
            TransactionId = transactionid;
            ProcessingCompleted = null;
            Status = TrackerStatus.Running;
            Documents = Documents ?? new List<TrackerDocumentDto>();
            PcdRequests = PcdRequests ?? new List<TrackerPcdRequestDto>();
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
            PcdRequests = tracker.PcdRequests;

            return Task.CompletedTask;
        }

        public Task<TrackerDeltasDto> SynchroniseDocuments(SynchroniseDocumentsArg arg)
        {
            if (Documents == null)
                Documents = new List<TrackerDocumentDto>();

            if (PcdRequests == null)
                PcdRequests = new List<TrackerPcdRequestDto>();

            var (createdDocuments, updatedDocuments, deletedDocuments) = GetDeltaDocuments(arg.Documents);
            var (createdPcdRequests, deletedPcdRequests) = GetDeltaPcdRequests(arg.PcdRequests);

            TrackerDeltasDto deltas = new TrackerDeltasDto
            {
                CreatedDocuments = CreateTrackerDocuments(createdDocuments),
                UpdatedDocuments = UpdateTrackerDocuments(updatedDocuments),
                DeletedDocuments = DeleteTrackerDocuments(deletedDocuments),
                CreatedPcdRequests = CreateTrackerPcdRequests(createdPcdRequests),
                DeletedPcdRequests = DeleteTrackerPcdRequests(deletedPcdRequests),
            };

            Status = TrackerStatus.DocumentsRetrieved;
            DocumentsRetrieved = DateTime.Now;
            VersionId = deltas.Any() ? VersionId+1 : Math.Max(VersionId, 1);

            Log(TrackerLogType.DocumentsSynchronised, null, $"{deltas.CreatedDocuments.Count} created, {deltas.UpdatedDocuments.Count} updated, {deltas.DeletedDocuments.Count} deleted");

            return Task.FromResult(deltas);
        }

        private (List<DocumentDto>, List<DocumentDto>, List<string>) GetDeltaDocuments(List<DocumentDto> documents)
        {
            var existingCmsDocumentIds = Documents.Select(doc => doc.CmsDocumentId).ToList();
            var newDocuments =
                documents
                    .Where(doc => !existingCmsDocumentIds.Contains(doc.DocumentId))
                    .ToList();

            var existingCmsDocumentIdVersions = Documents.Select(doc => (doc.CmsDocumentId, doc.CmsVersionId)).ToList();
            var updatedDocuments =
                documents
                    .Where(doc => existingCmsDocumentIds.Contains(doc.DocumentId))
                    .Where(doc => !existingCmsDocumentIdVersions.Contains((doc.DocumentId, doc.VersionId)))
                    .ToList();

            var deletedCmsDocumentIdsToRemove
                = Documents.Where(trackedDocument => !documents.Exists(x => x.DocumentId == trackedDocument.CmsDocumentId))
                    .Select(d => d.CmsDocumentId)
                    .ToList();

            return (newDocuments, updatedDocuments, deletedCmsDocumentIdsToRemove);
        }

        private (List<PcdRequestDto> createdPcdRequests, List<int> deletedPcdRequests) GetDeltaPcdRequests(List<PcdRequestDto> pcdRequests)
        {
            var existingPcdRequestIds = PcdRequests.Select(pcd => pcd.PcdRequestId).ToList();
            var newPcdRequests =
                pcdRequests
                    .Where(pcd => !existingPcdRequestIds.Contains(pcd.Id))
                    .ToList();

            var deletedPcdRequestIdsToRemove
                = PcdRequests.Where(pcd => !PcdRequests.Exists(x => pcd.PcdRequestId == x.PcdRequestId))
                    .Select(pcd => pcd.PcdRequestId)
                    .ToList();

            return (newPcdRequests, deletedPcdRequestIdsToRemove);
        }

        private List<TrackerDocumentDto> CreateTrackerDocuments(List<DocumentDto> createdDocuments)
        {
            List<TrackerDocumentDto> newDocuments = new List<TrackerDocumentDto>();

            foreach (var newDocument in createdDocuments)
            {
                TrackerDocumentDto trackerDocument 
                    = new TrackerDocumentDto
                    (
                        Guid.NewGuid(),
                        1,
                        newDocument.DocumentId,
                        newDocument.VersionId,
                        newDocument.CmsDocType,
                        newDocument.DocumentDate,
                        newDocument.FileName,
                        newDocument.PresentationFlags
                    );

                Documents.Add(trackerDocument);
                newDocuments.Add(trackerDocument);
                Log(TrackerLogType.DocumentRetrieved, trackerDocument.CmsDocumentId);
            }

            return newDocuments;
        }

        private List<TrackerDocumentDto> UpdateTrackerDocuments(List<DocumentDto> updatedDocuments)
        {
            List<TrackerDocumentDto> changedDocuments = new List<TrackerDocumentDto>();

            foreach (var updatedDocument in updatedDocuments)
            {
                TrackerDocumentDto trackerDocument = Documents.Find(d => d.CmsDocumentId == updatedDocument.DocumentId);

                trackerDocument.PolarisDocumentVersionId++;
                trackerDocument.CmsVersionId = updatedDocument.VersionId;
                trackerDocument.CmsDocType = updatedDocument.CmsDocType;
                trackerDocument.CmsFileCreatedDate = updatedDocument.DocumentDate;
                trackerDocument.CmsOriginalFileName = updatedDocument.FileName;
                trackerDocument.PresentationFlags = updatedDocument.PresentationFlags;

                changedDocuments.Add(trackerDocument);

                Log(TrackerLogType.DocumentRetrieved, trackerDocument.CmsDocumentId);
            }

            return changedDocuments;
        }

        private List<TrackerDocumentDto> DeleteTrackerDocuments(List<string> documentIdsToDelete)
        {
            var deleteDocuments 
                = Documents
                    .Where(d => documentIdsToDelete.Contains(d.CmsDocumentId))
                    .ToList();

            foreach (var document in deleteDocuments)
            {
                Log(TrackerLogType.DeletedDocument, document.CmsDocumentId);
                Documents.Remove(document);
            }

            return deleteDocuments;
        }

        private List<TrackerPcdRequestDto> CreateTrackerPcdRequests(List<PcdRequestDto> createdPcdRequests)
        {
            List<TrackerPcdRequestDto> newPcdRequests = new List<TrackerPcdRequestDto>();

            foreach (var newPcdRequest in createdPcdRequests)
            {
                TrackerPcdRequestDto trackerPcdRequest
                    = new TrackerPcdRequestDto
                    (
                        Guid.NewGuid(),
                        1,
                        newPcdRequest.Id,
                        newPcdRequest.PresentationFlags
                    );

                PcdRequests.Add(trackerPcdRequest);
                newPcdRequests.Add(trackerPcdRequest);
                Log(TrackerLogType.PcdRequestRetrieved, trackerPcdRequest.PcdRequestId.ToString());
            }

            return newPcdRequests;
        }

        private List<TrackerPcdRequestDto> DeleteTrackerPcdRequests(List<int> deletedPcdRequestIds)
        {
            var deletePcdRequests
                = PcdRequests
                    .Where(pcd => deletedPcdRequestIds.Contains(pcd.PcdRequestId))
                    .ToList();

            foreach (var pcdRequest in deletePcdRequests)
            {
                Log(TrackerLogType.DeletedPcdRequest, pcdRequest.PcdRequestId.ToString());
                PcdRequests.Remove(pcdRequest);
            }

            return deletePcdRequests;
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

        public Task RegisterUnexpectedPdfPcdRequestFailure(int id)
        {
            var pcdRequest = PcdRequests.Find(pcd => pcd.PcdRequestId == id);
            if (pcdRequest != null)
                pcdRequest.Status = TrackerDocumentStatus.UnableToConvertToPdf;

            Log(TrackerLogType.UnableToConvertPcdRequestToPdf, id.ToString());

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
            Log(TrackerLogType.DeletedDocument);

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