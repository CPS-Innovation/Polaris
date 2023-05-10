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

        [JsonProperty("defendantsAndCharges")]
        public TrackerDefendantsAndChargesDto DefendantsAndCharges { get; set; }

        [JsonProperty("logs")]
        public List<TrackerLogDto> Logs { get; set; }

        public Task Reset((DateTime t, string transactionId) arg)
        {
            TransactionId = arg.transactionId;
            ProcessingCompleted = null;
            Status = TrackerStatus.Running;
            CmsDocuments = CmsDocuments ?? new List<TrackerCmsDocumentDto>();
            PcdRequests = PcdRequests ?? new List<TrackerPcdRequestDto>();
            DefendantsAndCharges = DefendantsAndCharges ?? null;
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
            DefendantsAndCharges = tracker.DefendantsAndCharges;

            return Task.CompletedTask;
        }

        public Task<TrackerDeltasDto> GetCaseDocumentChanges((DateTime CurrentUtcDateTime, string CmsCaseUrn, long CmsCaseId, DocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges, Guid CorrelationId) arg)
        {
            CmsDocuments = CmsDocuments ?? new List<TrackerCmsDocumentDto>();
            PcdRequests = PcdRequests ?? new List<TrackerPcdRequestDto>();
            DefendantsAndCharges = DefendantsAndCharges ?? null;

            var (createdDocuments, updatedDocuments, deletedDocuments) = GetDeltaCmsDocuments(arg.CmsDocuments.ToList());
            var (createdPcdRequests, updatedPcdRequests, deletedPcdRequests) = GetDeltaPcdRequests(arg.PcdRequests.ToList());
            var (createdDefendantsAndCharges, updatedDefendantsAndCharges, deletedDefendantsAndCharges) = GetDeltaDefendantsAndCharges(arg.DefendantsAndCharges);

            TrackerDeltasDto deltas = new TrackerDeltasDto
            {
                CreatedCmsDocuments = CreateTrackerCmsDocuments(arg.CurrentUtcDateTime, createdDocuments),
                UpdatedCmsDocuments = UpdateTrackerCmsDocuments(arg.CurrentUtcDateTime, updatedDocuments),
                DeletedCmsDocuments = DeleteTrackerCmsDocuments(arg.CurrentUtcDateTime, deletedDocuments),
                CreatedPcdRequests = CreateTrackerPcdRequests(arg.CurrentUtcDateTime, createdPcdRequests),
                UpdatedPcdRequests = UpdateTrackerPcdRequests(arg.CurrentUtcDateTime, updatedPcdRequests),
                DeletedPcdRequests = DeleteTrackerPcdRequests(arg.CurrentUtcDateTime, deletedPcdRequests),
                CreatedDefendantsAndCharges = CreateTrackerDefendantsAndCharges(arg.CurrentUtcDateTime, createdDefendantsAndCharges),
                UpdatedDefendantsAndCharges = UpdateTrackerDefendantsAndCharges(arg.CurrentUtcDateTime, updatedDefendantsAndCharges),
                IsDeletedDefendantsAndCharges = DeleteTrackerDefendantsAndCharges(arg.CurrentUtcDateTime, deletedDefendantsAndCharges),
            };

            Status = TrackerStatus.DocumentsRetrieved;
            DocumentsRetrieved = DateTime.Now;
            VersionId = deltas.Any() ? VersionId+1 : Math.Max(VersionId, 1);

            var logMessage = $"CMS=({deltas.CreatedCmsDocuments.Count} created, {deltas.UpdatedCmsDocuments.Count} updated, {deltas.DeletedCmsDocuments.Count} deleted), " +
                             $"PCD=({deltas.CreatedPcdRequests.Count} created, {deltas.UpdatedPcdRequests.Count} updated, {deltas.DeletedPcdRequests.Count} deleted), " +
                             $"DAC=({(deltas.CreatedDefendantsAndCharges != null ? 1 : 0)} created, {(deltas.UpdatedDefendantsAndCharges != null ? 1 : 0)} updated, {(deltas.IsDeletedDefendantsAndCharges ? 1 : 0)} deleted)";
            Log(arg.CurrentUtcDateTime, TrackerLogType.DocumentsSynchronised, null, logMessage);

            return Task.FromResult(deltas);
        }

        private (List<DocumentDto>, List<DocumentDto>, List<string>) GetDeltaCmsDocuments(List<DocumentDto> incomingDocuments)
        {
            var newDocuments =
                (from incomingDocument in incomingDocuments
                 let cmsDocument = CmsDocuments.FirstOrDefault(doc => doc.DocumentId == incomingDocument.DocumentId)
                 where cmsDocument == null
                 select incomingDocument).ToList();

            var updatedDocuments =
                (from incomingDocument in incomingDocuments
                let cmsDocument = CmsDocuments.FirstOrDefault(doc => doc.DocumentId == incomingDocument.DocumentId)
                where cmsDocument != null 
                where cmsDocument.Status != TrackerDocumentStatus.Indexed || cmsDocument.CmsVersionId != incomingDocument.VersionId
                select incomingDocument).ToList();

            var deletedCmsDocumentIdsToRemove
                = CmsDocuments.Where(doc => !incomingDocuments.Any(incomingDoc => incomingDoc.DocumentId == doc.DocumentId))
                    .Select(doc => doc.DocumentId)
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

            var deletedPcdRequestIds
                = PcdRequests.Where(pcd => !incomingPcdRequests.Exists(incomingPcd => incomingPcd.Id == pcd.PcdRequest.Id))
                    .Select(pcd => pcd.PcdRequest.Id)
                    .ToList();

            return (newPcdRequests, updatedPcdRequests, deletedPcdRequestIds);
        }

        private (DefendantsAndChargesListDto createdDefendantsAndCharges, DefendantsAndChargesListDto updatedDefendantsAndCharges, bool deletedDefendantsAndCharges) GetDeltaDefendantsAndCharges(DefendantsAndChargesListDto incomingDefendantsAndCharges)
        {
            DefendantsAndChargesListDto newDefendantsAndCharges = null, updatedDefendantsAndCharges = null;

            if (DefendantsAndCharges == null && incomingDefendantsAndCharges != null) 
                newDefendantsAndCharges = incomingDefendantsAndCharges;

            if (DefendantsAndCharges != null && incomingDefendantsAndCharges != null)
            {
                if (JsonConvert.SerializeObject(DefendantsAndCharges.DefendantsAndCharges) != JsonConvert.SerializeObject(incomingDefendantsAndCharges))
                    updatedDefendantsAndCharges = incomingDefendantsAndCharges;
            }

            var deletedDefendantsAndCharges = (DefendantsAndCharges != null && incomingDefendantsAndCharges == null);

            return (newDefendantsAndCharges, updatedDefendantsAndCharges, deletedDefendantsAndCharges);
        }

        private List<TrackerCmsDocumentDto> CreateTrackerCmsDocuments(DateTime t, List<DocumentDto> createdDocuments)
        {
            var newDocuments = new List<TrackerCmsDocumentDto>();

            foreach (var newDocument in createdDocuments)
            {
                var trackerDocument 
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
                Log(t, TrackerLogType.CmsDocumentRetrieved, trackerDocument.DocumentId);
            }

            return newDocuments;
        }

        private List<TrackerCmsDocumentDto> UpdateTrackerCmsDocuments(DateTime t, List<DocumentDto> updatedDocuments)
        {
            var changedDocuments = new List<TrackerCmsDocumentDto>();

            foreach (var updatedDocument in updatedDocuments)
            {
                var trackerDocument = CmsDocuments.Find(d => d.DocumentId == updatedDocument.DocumentId);

                trackerDocument.PolarisDocumentVersionId++;
                trackerDocument.CmsVersionId = updatedDocument.VersionId;
                trackerDocument.CmsDocType = updatedDocument.CmsDocType;
                trackerDocument.CmsFileCreatedDate = updatedDocument.DocumentDate;
                trackerDocument.CmsOriginalFileName = updatedDocument.FileName;
                trackerDocument.PresentationFlags = updatedDocument.PresentationFlags;

                changedDocuments.Add(trackerDocument);

                Log(t, TrackerLogType.CmsDocumentUpdated, trackerDocument.DocumentId);
            }

            return changedDocuments;
        }

        private List<TrackerCmsDocumentDto> DeleteTrackerCmsDocuments(DateTime t, List<string> documentIdsToDelete)
        {
            var deleteDocuments 
                = CmsDocuments
                    .Where(d => documentIdsToDelete.Contains(d.DocumentId))
                    .ToList();

            foreach (var document in deleteDocuments)
            {
                Log(t, TrackerLogType.CmsDocumentDeleted, document.DocumentId);
                CmsDocuments.Remove(document);
            }

            return deleteDocuments;
        }

        private List<TrackerPcdRequestDto> CreateTrackerPcdRequests(DateTime t, List<PcdRequestDto> createdPcdRequests)
        {
            var newPcdRequests = new List<TrackerPcdRequestDto>();

            foreach (var newPcdRequest in createdPcdRequests)
            {
                var trackerPcdRequest = new TrackerPcdRequestDto(Guid.NewGuid(), 1, newPcdRequest);
                PcdRequests.Add(trackerPcdRequest);
                newPcdRequests.Add(trackerPcdRequest);
                Log(t, TrackerLogType.PcdRequestRetrieved, trackerPcdRequest.PcdRequest.Id.ToString());
            }

            return newPcdRequests;
        }

        private List<TrackerPcdRequestDto> UpdateTrackerPcdRequests(DateTime t, List<PcdRequestDto> updatedPcdRequests)
        {
            var changedPcdRequests = new List<TrackerPcdRequestDto>();

            foreach (var updatedPcdRequest in updatedPcdRequests)
            {
                var trackerPcdRequest = PcdRequests.Find(pcd => pcd.PcdRequest.Id == updatedPcdRequest.Id);

                trackerPcdRequest.PolarisDocumentVersionId++;
                trackerPcdRequest.PresentationFlags = updatedPcdRequest.PresentationFlags;

                changedPcdRequests.Add(trackerPcdRequest);

                Log(t, TrackerLogType.PcdRequestUpdated, trackerPcdRequest.PcdRequest.Id.ToString());
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
                Log(t, TrackerLogType.PcdRequestDeleted, pcdRequest.PcdRequest.Id.ToString());
                PcdRequests.Remove(pcdRequest);
            }

            return deletePcdRequests;
        }

        private TrackerDefendantsAndChargesDto CreateTrackerDefendantsAndCharges(DateTime t, DefendantsAndChargesListDto createdDefendantsAndCharges)
        {
            if(createdDefendantsAndCharges != null)
            {
                DefendantsAndCharges = new TrackerDefendantsAndChargesDto(Guid.NewGuid(), 1, createdDefendantsAndCharges);

                Log(t, TrackerLogType.DefendantAndChargesRetrieved, DefendantsAndCharges.DefendantsAndCharges.CaseId.ToString());

                return DefendantsAndCharges;
            }

            return null;
        }

        private TrackerDefendantsAndChargesDto UpdateTrackerDefendantsAndCharges(DateTime t, DefendantsAndChargesListDto updatedDefendantsAndCharges)
        {
            if(updatedDefendantsAndCharges != null)
            {
                DefendantsAndCharges.DefendantsAndCharges = updatedDefendantsAndCharges;
                DefendantsAndCharges.PolarisDocumentVersionId++;

                Log(t, TrackerLogType.DefendantAndChargesUpdated, DefendantsAndCharges.DefendantsAndCharges.CaseId.ToString());

                return DefendantsAndCharges;
            }

            return null;
        }

        private bool DeleteTrackerDefendantsAndCharges(DateTime t, bool deletedDefendantsAndCharges)
        {
            if(deletedDefendantsAndCharges) 
            {
                DefendantsAndCharges = null;
                Log(t, TrackerLogType.DefendantAndChargesDeleted, deletedDefendantsAndCharges.ToString());
            }

            return deletedDefendantsAndCharges;
        }

        public Task RegisterPdfBlobName(RegisterPdfBlobNameArg arg)
        {
            var document = GetBaseTracker(arg.DocumentId);
            document.PdfBlobName = arg.BlobName;
            document.Status = TrackerDocumentStatus.PdfUploadedToBlob;
            document.IsPdfAvailable = true;

            Log(arg.CurrentUtcDateTime, TrackerLogType.RegisteredPdfBlobName, arg.DocumentId);

            return Task.CompletedTask;
        }

        public Task RegisterBlobAlreadyProcessed(RegisterPdfBlobNameArg arg)
        {
            var document = GetBaseTracker(arg.DocumentId);
            document.PdfBlobName = arg.BlobName;
            document.Status = TrackerDocumentStatus.DocumentAlreadyProcessed;

            Log(arg.CurrentUtcDateTime, TrackerLogType.DocumentAlreadyProcessed, arg.DocumentId);

            return Task.CompletedTask;
        }

        public Task RegisterStatus((DateTime t, string documentId, TrackerDocumentStatus status, TrackerLogType logType) arg)
        {
            var document = GetBaseTracker(arg.documentId);
            document.Status = arg.status;

            Log(arg.t, arg.logType, arg.documentId);

            return Task.CompletedTask;
        }

        public Task RegisterCompleted((DateTime t, bool success) arg)
        {
            Status = arg.success ? TrackerStatus.Completed : TrackerStatus.Failed;
            ProcessingCompleted = arg.t;
            Log(arg.t, arg.success ? TrackerLogType.Completed : TrackerLogType.Failed);

            return Task.CompletedTask;
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

        private BaseTrackerDocumentDto GetBaseTracker(string documentId)
        {
            var cmsDocument = CmsDocuments.Find(doc => doc.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            if (cmsDocument != null)
            {
                return cmsDocument;
            }

            var pcdRequest = PcdRequests.Find(pcd => pcd.DocumentId.Equals(documentId, StringComparison.OrdinalIgnoreCase));
            if (pcdRequest != null)
            {
                return pcdRequest;
            }

            if (DefendantsAndCharges != null)
                return DefendantsAndCharges;

            return null;
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