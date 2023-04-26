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
        public List<TrackerCmsDocumentDto> CmsDocuments { get; set; }

        [JsonProperty("pcdRequests")]
        public List<TrackerPcdRequestDto> PcdRequests { get; set; }

        [JsonProperty("logs")]
        public List<TrackerLogDto> Logs { get; set; }

        public Task Reset(string transactionid)
        {
            TransactionId = transactionid;
            ProcessingCompleted = null;
            Status = TrackerStatus.Running;
            CmsDocuments = CmsDocuments ?? new List<TrackerCmsDocumentDto>();
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

            CmsDocuments = tracker.CmsDocuments;
            PcdRequests = tracker.PcdRequests;

            return Task.CompletedTask;
        }

        public Task<TrackerDeltasDto> SynchroniseDocuments(SynchroniseDocumentsArg arg)
        {
            if (CmsDocuments == null)
                CmsDocuments = new List<TrackerCmsDocumentDto>();

            if (PcdRequests == null)
                PcdRequests = new List<TrackerPcdRequestDto>();

            var (createdDocuments, updatedDocuments, deletedDocuments) = GetDeltaCmsDocuments(arg.Documents);
            var (createdPcdRequests, updatedPcdRequests, deletedPcdRequests) = GetDeltaPcdRequests(arg.PcdRequests);

            TrackerDeltasDto deltas = new TrackerDeltasDto
            {
                CreatedDocuments = CreateTrackerDocuments(createdDocuments),
                UpdatedDocuments = UpdateTrackerDocuments(updatedDocuments),
                DeletedDocuments = DeleteTrackerDocuments(deletedDocuments),
                CreatedPcdRequests = CreateTrackerPcdRequests(createdPcdRequests),
                UpdatedPcdRequests = UpdateTrackerPcdRequests(updatedPcdRequests),
                DeletedPcdRequests = DeleteTrackerPcdRequests(deletedPcdRequests),
            };

            Status = TrackerStatus.DocumentsRetrieved;
            DocumentsRetrieved = DateTime.Now;
            VersionId = deltas.Any() ? VersionId+1 : Math.Max(VersionId, 1);

            Log(TrackerLogType.DocumentsSynchronised, null, $"{deltas.CreatedDocuments.Count} CMS Documents created, {deltas.UpdatedDocuments.Count} updated, {deltas.DeletedDocuments.Count} deleted");
            Log(TrackerLogType.DocumentsSynchronised, null, $"{deltas.CreatedPcdRequests.Count} PCD Requests created, {deltas.UpdatedPcdRequests.Count} updated {deltas.DeletedPcdRequests.Count} deleted");

            return Task.FromResult(deltas);
        }

        private (List<DocumentDto>, List<DocumentDto>, List<string>) GetDeltaCmsDocuments(List<DocumentDto> incomingDocuments)
        {
            var newDocuments =
                (from incomingDocument in incomingDocuments
                 let cmsDocument = CmsDocuments.FirstOrDefault(doc => doc.CmsDocumentId == incomingDocument.DocumentId)
                 where cmsDocument == null
                 select incomingDocument).ToList();

            var updatedDocuments =
                (from incomingDocument in incomingDocuments
                let cmsDocument = CmsDocuments.FirstOrDefault(doc => doc.CmsDocumentId == incomingDocument.DocumentId)
                where cmsDocument != null 
                where cmsDocument.Status != TrackerDocumentStatus.Indexed || cmsDocument.CmsVersionId != incomingDocument.VersionId
                select incomingDocument).ToList();

            var deletedCmsDocumentIdsToRemove
                = CmsDocuments.Where(doc => !incomingDocuments.Any(incomingDoc => incomingDoc.DocumentId == doc.CmsDocumentId))
                    .Select(doc => doc.CmsDocumentId)
                    .ToList();

            return (newDocuments, updatedDocuments, deletedCmsDocumentIdsToRemove);
        }

        private (List<PcdRequestDto> createdPcdRequests, List<PcdRequestDto> updatedPcdRequests, List<int> deletedPcdRequests) GetDeltaPcdRequests(List<PcdRequestDto> incomingPcdRequests)
        {
            var newPcdRequests =
                incomingPcdRequests
                    .Where(incomingPcd => !PcdRequests.Any(pcd => pcd.PcdRequest.Id == incomingPcd.Id))
                    .ToList();

            var updatedPcdRequests =
                incomingPcdRequests
                    .Where(incomingPcd => PcdRequests.Any(pcd => pcd.PcdRequest.Id == incomingPcd.Id && pcd.Status != TrackerDocumentStatus.Indexed))
                    .ToList();

            var deletedPcdRequestIdsToRemove
                = PcdRequests.Where(pcd => !incomingPcdRequests.Exists(incomingPcd => incomingPcd.Id == pcd.PcdRequest.Id))
                    .Select(pcd => pcd.PcdRequest.Id)
                    .ToList();

            return (newPcdRequests, updatedPcdRequests, deletedPcdRequestIdsToRemove);
        }

        private List<TrackerCmsDocumentDto> CreateTrackerDocuments(List<DocumentDto> createdDocuments)
        {
            List<TrackerCmsDocumentDto> newDocuments = new List<TrackerCmsDocumentDto>();

            foreach (var newDocument in createdDocuments)
            {
                TrackerCmsDocumentDto trackerDocument 
                    = new TrackerCmsDocumentDto
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

                CmsDocuments.Add(trackerDocument);
                newDocuments.Add(trackerDocument);
                Log(TrackerLogType.DocumentRetrieved, trackerDocument.CmsDocumentId);
            }

            return newDocuments;
        }

        private List<TrackerCmsDocumentDto> UpdateTrackerDocuments(List<DocumentDto> updatedDocuments)
        {
            List<TrackerCmsDocumentDto> changedDocuments = new List<TrackerCmsDocumentDto>();

            foreach (var updatedDocument in updatedDocuments)
            {
                TrackerCmsDocumentDto trackerDocument = CmsDocuments.Find(d => d.CmsDocumentId == updatedDocument.DocumentId);

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

        private List<TrackerCmsDocumentDto> DeleteTrackerDocuments(List<string> documentIdsToDelete)
        {
            var deleteDocuments 
                = CmsDocuments
                    .Where(d => documentIdsToDelete.Contains(d.CmsDocumentId))
                    .ToList();

            foreach (var document in deleteDocuments)
            {
                Log(TrackerLogType.DeletedDocument, document.CmsDocumentId);
                CmsDocuments.Remove(document);
            }

            return deleteDocuments;
        }

        private List<TrackerPcdRequestDto> CreateTrackerPcdRequests(List<PcdRequestDto> createdPcdRequests)
        {
            List<TrackerPcdRequestDto> newPcdRequests = new List<TrackerPcdRequestDto>();

            foreach (var newPcdRequest in createdPcdRequests)
            {
                TrackerPcdRequestDto trackerPcdRequest = new TrackerPcdRequestDto(Guid.NewGuid(), 1, newPcdRequest);
                PcdRequests.Add(trackerPcdRequest);
                newPcdRequests.Add(trackerPcdRequest);
                Log(TrackerLogType.PcdRequestRetrieved, trackerPcdRequest.PcdRequest.Id.ToString());
            }

            return newPcdRequests;
        }

        private List<TrackerPcdRequestDto> UpdateTrackerPcdRequests(List<PcdRequestDto> updatedPcdRequests)
        {
            List<TrackerPcdRequestDto> changedPcdRequests = new List<TrackerPcdRequestDto>();

            foreach (var updatedPcdRequest in updatedPcdRequests)
            {
                TrackerPcdRequestDto trackerPcdRequest = PcdRequests.Find(d => d.PcdRequest.Id == updatedPcdRequest.Id);

                trackerPcdRequest.PolarisDocumentVersionId++;
                trackerPcdRequest.PresentationFlags = updatedPcdRequest.PresentationFlags;

                changedPcdRequests.Add(trackerPcdRequest);

                Log(TrackerLogType.PcdRequestRetrieved, trackerPcdRequest.PcdRequest.Id.ToString());
            }

            return changedPcdRequests;
        }

        private List<TrackerPcdRequestDto> DeleteTrackerPcdRequests(List<int> deletedPcdRequestIds)
        {
            var deletePcdRequests
                = PcdRequests
                    .Where(pcd => deletedPcdRequestIds.Contains(pcd.PcdRequest.Id))
                    .ToList();

            foreach (var pcdRequest in deletePcdRequests)
            {
                Log(TrackerLogType.DeletedPcdRequest, pcdRequest.PcdRequest.Id.ToString());
                PcdRequests.Remove(pcdRequest);
            }

            return deletePcdRequests;
        }

        public Task RegisterPdfBlobName(RegisterPdfBlobNameArg arg)
        {
            var document = CmsDocuments.Find(document => document.CmsDocumentId.Equals(arg.DocumentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
            {
                document.PdfBlobName = arg.BlobName;
                document.Status = TrackerDocumentStatus.PdfUploadedToBlob;
                document.IsPdfAvailable = true;
            }
            else
            {
                var pcdRequest = PcdRequests.Find(pcdRequest => pcdRequest.CmsDocumentId.Equals(arg.DocumentId, StringComparison.OrdinalIgnoreCase));
                if(pcdRequest != null)
                {
                    pcdRequest.PdfBlobName = arg.BlobName;
                    pcdRequest.Status = TrackerDocumentStatus.PdfUploadedToBlob;
                    pcdRequest.IsPdfAvailable = true;
                }
            }

            Log(TrackerLogType.RegisteredPdfBlobName, arg.DocumentId);

            return Task.CompletedTask;
        }

        public Task RegisterBlobAlreadyProcessed(RegisterPdfBlobNameArg arg)
        {
            var document = CmsDocuments.Find(document => document.CmsDocumentId.Equals(arg.DocumentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
            {
                document.PdfBlobName = arg.BlobName;
                document.Status = TrackerDocumentStatus.DocumentAlreadyProcessed;
            }
            else
            {
                var pcdRequest = PcdRequests.Find(pcdRequest => pcdRequest.CmsDocumentId.Equals(arg.DocumentId, StringComparison.OrdinalIgnoreCase));
                if (pcdRequest != null)
                {
                    pcdRequest.PdfBlobName = arg.BlobName;
                    pcdRequest.Status = TrackerDocumentStatus.DocumentAlreadyProcessed;
                }
            }

            Log(TrackerLogType.DocumentAlreadyProcessed, arg.DocumentId);

            return Task.CompletedTask;
        }

        public Task RegisterUnableToConvertDocumentToPdf(string documentId)
        {
            var document = CmsDocuments.Find(document => document.CmsDocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
            {
                document.Status = TrackerDocumentStatus.UnableToConvertToPdf;
            }
            else
            {
                var pcdRequest = PcdRequests.Find(pcdRequest => pcdRequest.CmsDocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
                if (pcdRequest != null)
                {
                    pcdRequest.Status = TrackerDocumentStatus.UnableToConvertToPdf;
                }
            }

            Log(TrackerLogType.UnableToConvertDocumentToPdf, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterUnexpectedPdfPcdRequestFailure(int id)
        {
            var pcdRequest = PcdRequests.Find(pcd => pcd.PcdRequest.Id == id);
            if (pcdRequest != null)
                pcdRequest.Status = TrackerDocumentStatus.UnableToConvertToPdf;

            Log(TrackerLogType.UnableToConvertPcdRequestToPdf, id.ToString());

            return Task.CompletedTask;
        }

        public Task RegisterUnexpectedPdfDocumentFailure(string documentId)
        {
            var document = CmsDocuments.Find(document => document.CmsDocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
            {
                document.Status = TrackerDocumentStatus.UnexpectedFailure;
            }
            else
            {
                var pcdRequest = PcdRequests.Find(document => document.CmsDocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
                if (pcdRequest != null)
                    pcdRequest.Status = TrackerDocumentStatus.UnexpectedFailure;
            }

            Log(TrackerLogType.UnexpectedDocumentFailure, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterIndexed(string documentId)
        {
            var document = CmsDocuments.Find(document => document.CmsDocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
            {
                document.Status = TrackerDocumentStatus.Indexed;
            }
            else
            {
                var pcdRequest = PcdRequests.Find(document => document.CmsDocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
                if (pcdRequest != null)
                    pcdRequest.Status = TrackerDocumentStatus.Indexed;
            }

            Log(TrackerLogType.Indexed, documentId);

            return Task.CompletedTask;
        }

        public Task RegisterOcrAndIndexFailure(string documentId)
        {
            var document = CmsDocuments.Find(document => document.CmsDocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
            {
                document.Status = TrackerDocumentStatus.OcrAndIndexFailure;
            }
            else
            {
                var pcdRequest = PcdRequests.Find(document => document.CmsDocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
                if (pcdRequest != null)
                    pcdRequest.Status = TrackerDocumentStatus.OcrAndIndexFailure;
            }

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

        public Task<List<TrackerCmsDocumentDto>> GetDocuments()
        {
            return Task.FromResult(CmsDocuments);
        }

        public Task ClearDocuments()
        {
            CmsDocuments.Clear();

            return Task.CompletedTask;
        }

        public Task<bool> AllDocumentsFailed()
        {
            var statuses = 
                CmsDocuments
                    .Select(doc => doc.Status)
                    .Concat(PcdRequests.Select(pcd => pcd.Status))
                    .ToList();

            return Task.FromResult(
                statuses.All(s => s is TrackerDocumentStatus.UnableToConvertToPdf or TrackerDocumentStatus.UnexpectedFailure));
        }

        private void ClearState(TrackerStatus status)
        {
            Status = status;
            CmsDocuments = new List<TrackerCmsDocumentDto>();
            PcdRequests = new List<TrackerPcdRequestDto>();
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