using Common.Dto.Case;
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
using System.Threading.Tasks;

namespace coordinator.Functions.DurableEntity.Entity
{
    // n.b. Entity proxy interface methods must define at most one argument for operation input.
    // (A single tuple is acceptable)
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

        public Task Reset((DateTime t, string transactionId) arg)
        {
            TransactionId = arg.transactionId;
            ProcessingCompleted = null;
            Status = TrackerStatus.Running;
            CmsDocuments = CmsDocuments ?? new List<TrackerCmsDocumentDto>();
            PcdRequests = PcdRequests ?? new List<TrackerPcdRequestDto>();
            Logs = Logs ?? new List<TrackerLogDto>();

            Log(arg.t, TrackerLogType.Initialised);

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

        public Task<TrackerDeltasDto> SynchroniseDocuments((DateTime CurrentUtcDateTime, string CmsCaseUrn, long CmsCaseId, DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantAndChargesDto[] DefendantsAndCharges, Guid CorrelationId) arg)
        {
            if (CmsDocuments == null)
                CmsDocuments = new List<TrackerCmsDocumentDto>();

            if (PcdRequests == null)
                PcdRequests = new List<TrackerPcdRequestDto>();

            var (createdDocuments, updatedDocuments, deletedDocuments) = GetDeltaCmsDocuments(arg.CmsDocuments.ToList());
            var (createdPcdRequests, updatedPcdRequests, deletedPcdRequests) = GetDeltaPcdRequests(arg.PcdRequests.ToList());

            TrackerDeltasDto deltas = new TrackerDeltasDto
            {
                CreatedCmsDocuments = CreateTrackerDocuments(arg.CurrentUtcDateTime, createdDocuments),
                UpdatedCmsDocuments = UpdateTrackerDocuments(arg.CurrentUtcDateTime, updatedDocuments),
                DeletedCmsDocuments = DeleteTrackerDocuments(arg.CurrentUtcDateTime, deletedDocuments),
                CreatedPcdRequests = CreateTrackerPcdRequests(arg.CurrentUtcDateTime, createdPcdRequests),
                UpdatedPcdRequests = UpdateTrackerPcdRequests(arg.CurrentUtcDateTime, updatedPcdRequests),
                DeletedPcdRequests = DeleteTrackerPcdRequests(arg.CurrentUtcDateTime, deletedPcdRequests),
            };

            Status = TrackerStatus.DocumentsRetrieved;
            DocumentsRetrieved = DateTime.Now;
            VersionId = deltas.Any() ? VersionId+1 : Math.Max(VersionId, 1);

            Log(arg.CurrentUtcDateTime, TrackerLogType.DocumentsSynchronised, null, $"{deltas.CreatedCmsDocuments.Count} CMS Documents created, {deltas.UpdatedCmsDocuments.Count} updated, {deltas.DeletedCmsDocuments.Count} deleted");
            Log(arg.CurrentUtcDateTime, TrackerLogType.DocumentsSynchronised, null, $"{deltas.CreatedPcdRequests.Count} PCD Requests created, {deltas.UpdatedPcdRequests.Count} updated {deltas.DeletedPcdRequests.Count} deleted");

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

        private List<TrackerCmsDocumentDto> CreateTrackerDocuments(DateTime t, List<DocumentDto> createdDocuments)
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
                Log(t, TrackerLogType.DocumentRetrieved, trackerDocument.CmsDocumentId);
            }

            return newDocuments;
        }

        private List<TrackerCmsDocumentDto> UpdateTrackerDocuments(DateTime t, List<DocumentDto> updatedDocuments)
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

                Log(t, TrackerLogType.DocumentRetrieved, trackerDocument.CmsDocumentId);
            }

            return changedDocuments;
        }

        private List<TrackerCmsDocumentDto> DeleteTrackerDocuments(DateTime t, List<string> documentIdsToDelete)
        {
            var deleteDocuments 
                = CmsDocuments
                    .Where(d => documentIdsToDelete.Contains(d.CmsDocumentId))
                    .ToList();

            foreach (var document in deleteDocuments)
            {
                Log(t, TrackerLogType.DeletedDocument, document.CmsDocumentId);
                CmsDocuments.Remove(document);
            }

            return deleteDocuments;
        }

        private List<TrackerPcdRequestDto> CreateTrackerPcdRequests(DateTime t, List<PcdRequestDto> createdPcdRequests)
        {
            List<TrackerPcdRequestDto> newPcdRequests = new List<TrackerPcdRequestDto>();

            foreach (var newPcdRequest in createdPcdRequests)
            {
                TrackerPcdRequestDto trackerPcdRequest = new TrackerPcdRequestDto(Guid.NewGuid(), 1, newPcdRequest);
                PcdRequests.Add(trackerPcdRequest);
                newPcdRequests.Add(trackerPcdRequest);
                Log(t, TrackerLogType.PcdRequestRetrieved, trackerPcdRequest.PcdRequest.Id.ToString());
            }

            return newPcdRequests;
        }

        private List<TrackerPcdRequestDto> UpdateTrackerPcdRequests(DateTime t, List<PcdRequestDto> updatedPcdRequests)
        {
            List<TrackerPcdRequestDto> changedPcdRequests = new List<TrackerPcdRequestDto>();

            foreach (var updatedPcdRequest in updatedPcdRequests)
            {
                TrackerPcdRequestDto trackerPcdRequest = PcdRequests.Find(d => d.PcdRequest.Id == updatedPcdRequest.Id);

                trackerPcdRequest.PolarisDocumentVersionId++;
                trackerPcdRequest.PresentationFlags = updatedPcdRequest.PresentationFlags;

                changedPcdRequests.Add(trackerPcdRequest);

                Log(t, TrackerLogType.PcdRequestRetrieved, trackerPcdRequest.PcdRequest.Id.ToString());
            }

            return changedPcdRequests;
        }

        private List<TrackerPcdRequestDto> DeleteTrackerPcdRequests(DateTime t, List<int> deletedPcdRequestIds)
        {
            var deletePcdRequests
                = PcdRequests
                    .Where(pcd => deletedPcdRequestIds.Contains(pcd.PcdRequest.Id))
                    .ToList();

            foreach (var pcdRequest in deletePcdRequests)
            {
                Log(t, TrackerLogType.DeletedPcdRequest, pcdRequest.PcdRequest.Id.ToString());
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

            Log(arg.CurrentUtcDateTime, TrackerLogType.RegisteredPdfBlobName, arg.DocumentId);

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

            Log(arg.CurrentUtcDateTime, TrackerLogType.DocumentAlreadyProcessed, arg.DocumentId);

            return Task.CompletedTask;
        }

        public Task RegisterUnableToConvertDocumentToPdf((DateTime t, string documentId) arg)
        {
            var document = CmsDocuments.Find(document => document.CmsDocumentId.Equals(arg.documentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
            {
                document.Status = TrackerDocumentStatus.UnableToConvertToPdf;
            }
            else
            {
                var pcdRequest = PcdRequests.Find(pcdRequest => pcdRequest.CmsDocumentId.Equals(arg.documentId, StringComparison.OrdinalIgnoreCase));
                if (pcdRequest != null)
                {
                    pcdRequest.Status = TrackerDocumentStatus.UnableToConvertToPdf;
                }
            }

            Log(arg.t, TrackerLogType.UnableToConvertDocumentToPdf, arg.documentId);

            return Task.CompletedTask;
        }

        public Task RegisterUnexpectedPdfPcdRequestFailure((DateTime t, int id) arg)
        {
            var pcdRequest = PcdRequests.Find(pcd => pcd.PcdRequest.Id == arg.id);
            if (pcdRequest != null)
                pcdRequest.Status = TrackerDocumentStatus.UnableToConvertToPdf;

            Log(arg.t, TrackerLogType.UnableToConvertPcdRequestToPdf, arg.id.ToString());

            return Task.CompletedTask;
        }

        public Task RegisterUnexpectedPdfDocumentFailure((DateTime t, string documentId) arg)
        {
            var document = CmsDocuments.Find(document => document.CmsDocumentId.Equals(arg.documentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
            {
                document.Status = TrackerDocumentStatus.UnexpectedFailure;
            }
            else
            {
                var pcdRequest = PcdRequests.Find(document => document.CmsDocumentId.Equals(arg.documentId, StringComparison.OrdinalIgnoreCase));
                if (pcdRequest != null)
                    pcdRequest.Status = TrackerDocumentStatus.UnexpectedFailure;
            }

            Log(arg.t, TrackerLogType.UnexpectedDocumentFailure, arg.documentId);

            return Task.CompletedTask;
        }

        public Task RegisterIndexed((DateTime t, string documentId) arg)
        {
            var document = CmsDocuments.Find(document => document.CmsDocumentId.Equals(arg.documentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
            {
                document.Status = TrackerDocumentStatus.Indexed;
            }
            else
            {
                var pcdRequest = PcdRequests.Find(document => document.CmsDocumentId.Equals(arg.documentId, StringComparison.OrdinalIgnoreCase));
                if (pcdRequest != null)
                    pcdRequest.Status = TrackerDocumentStatus.Indexed;
            }

            Log(arg.t, TrackerLogType.Indexed, arg.documentId);

            return Task.CompletedTask;
        }

        public Task RegisterOcrAndIndexFailure((DateTime t, string documentId) arg)
        {
            var document = CmsDocuments.Find(document => document.CmsDocumentId.Equals(arg.documentId, StringComparison.OrdinalIgnoreCase));
            if (document != null)
            {
                document.Status = TrackerDocumentStatus.OcrAndIndexFailure;
            }
            else
            {
                var pcdRequest = PcdRequests.Find(document => document.CmsDocumentId.Equals(arg.documentId, StringComparison.OrdinalIgnoreCase));
                if (pcdRequest != null)
                    pcdRequest.Status = TrackerDocumentStatus.OcrAndIndexFailure;
            }

            Log(arg.t, TrackerLogType.OcrAndIndexFailure, arg.documentId);

            return Task.CompletedTask;
        }

        public Task RegisterCompleted(DateTime t)
        {
            Status = TrackerStatus.Completed;
            ProcessingCompleted = DateTime.Now;
            Log(t, TrackerLogType.Completed);

            return Task.CompletedTask;
        }

        public Task RegisterFailed(DateTime t)
        {
            Status = TrackerStatus.Failed;
            ProcessingCompleted = DateTime.Now;
            Log(t, TrackerLogType.Failed);

            return Task.CompletedTask;
        }

        public Task RegisterDeleted(DateTime t)
        {
            ClearState(TrackerStatus.Deleted);
            ProcessingCompleted = DateTime.Now;
            Log(t, TrackerLogType.DeletedDocument);

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

        public Task<bool> AnyDocumentsFailed()
        {
            var statuses =
                CmsDocuments
                    .Select(doc => doc.Status)
                    .Concat(PcdRequests.Select(pcd => pcd.Status))
                    .ToList();

            return Task.FromResult(
                statuses.Any(s => s is TrackerDocumentStatus.UnableToConvertToPdf or TrackerDocumentStatus.UnexpectedFailure));
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

        private void Log(DateTime t, TrackerLogType status, string cmsDocumentId = null, string description = null)
        {
            TrackerLogDto item = new TrackerLogDto
            {
                LogType = status.ToString(),
                TimeStamp = t.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffzzz"),
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